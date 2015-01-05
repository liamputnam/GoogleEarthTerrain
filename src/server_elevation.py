from flask import Flask, request
import json
import requests

app = Flask(__name__,static_url_path='')

# google maps elevation api url stem
url_stem = 'https://maps.googleapis.com/maps/api/elevation/json'

# kilometers per degree of latitude (this is constant)
km_per_degree_latitude = 111.2

def get_height_row(start, end, num_samples_per_row):
	# construct url
	path = 'path=%f,%f|%f,%f' % (start[0], start[1], end[0], end[1])
	samples = 'samples=%d' % num_samples_per_row
	api_key = 'key=AIzaSyAF4Bkkocrsf9tbab2UDsUfP8P5mSUpupc'
	url = '%s?%s&%s&%s' % (url_stem, path, samples, api_key)

	ret_val = {}
	ret_val['height_row'] = []

	# fire it off
	elevation_data = requests.get(url).json()
	print('elevation_data ', elevation_data)
	if not 'results' in elevation_data or len(elevation_data['results']) == 0:
		ret_val['error'] = elevation_data['error_message']
		return ret_val
	# loop over results

	for result in elevation_data['results']:
		ret_val['height_row'].append(result['elevation'])

	return ret_val


# get a 2d-grid of vertices around the center
@app.route('/elevation')
def get_vertex_grid():
	center = request.args.get('center').split(',')
	radius = request.args.get('radius')
	width  = request.args.get('width')
	length = request.args.get('length')

	#sanitize url
	if len(center) is not 2:
		print('Error, malformed url')
		return 'Error, malformed url'
	try:
		center[0] = float(center[0])
		center[1] = float(center[1])
		diameter  = int(radius) * 2
		width     = int(width)
		length    = int(length)
	except:
		print('Error, expected numeric parameters')
		return 'Error, expected numeric parameters'

	if length > 256:
		length = 256
	if width > 256:
		width = 256

	# latitudes are between -90 and 90
	top_latitude    = ((center[0] + (diameter / 2 / km_per_degree_latitude) + 90) % 180) - 90
	bottom_latitude = ((center[0] - (diameter / 2 / km_per_degree_latitude) + 90) % 180) - 90

	# longitudes are between 0 and 180
	right_longitude = center[1] + (diameter / 2 / km_per_degree_latitude) % 180
	left_longitude  = center[1] - (diameter / 2 / km_per_degree_latitude) % 180

	# get distance between grid bounds
	dy = (top_latitude - bottom_latitude) / diameter
	#dx = ((right_longitude - left_longitude) % 180) / diameter

	# loop over rows
	height_grid = []
	for i in range(0, length):
		row_latitude = top_latitude - (i * dy)
		height_row = get_height_row((row_latitude, left_longitude), (row_latitude, right_longitude), width)

		# if google barfed, barf
		if len(height_row['height_row']) is 0:
			print("empty height row")
			error = {}
			error['error'] = height_row['error']
			return json.dumps(error)

		height_grid.append(height_row['height_row'])

	print(height_grid)
	return json.dumps(height_grid)

if __name__ == '__main__':
#	app.debug = True
#	app.run()
	app.run('0.0.0.0')