import requests 
import json

# google maps elevation api url stem
url_stem = 'https://maps.googleapis.com/maps/api/elevation/json'

# kilometers per degree of latitude (this is constant)
km_per_degree_latitude = 111.2

# number of kilometers across the side of the grid
grid_length_in_km = 2

# number of samples to take per row of the grid
num_samples_per_row = 2

# get the row of vertices, from start to end
def get_vertex_row(start, end):

	# construct url
	path = 'path=%f,%f|%f,%f' % (start[0], start[1], end[0], end[1])
	samples = 'samples=%d' % num_samples_per_row
	url = '%s?%s&%s' % (url_stem, path, samples)

	# fire it off
	# TODO: check for failure here
	elevation_data = requests.get(url).json()

	# loop over results
	vertex_row = []
	for result in elevation_data['results']:
		# make a 3-tuple for each result (x,y,z)
		vertex_row.append((result['location']['lat'], result['location']['lng'], result['elevation']))

	return vertex_row

# get a 2d-grid of vertices around the center
def get_vertex_grid(center):

	# latitudes are between -90 and 90
	top_latitude    = ((center[0] + (grid_length_in_km / 2 / km_per_degree_latitude) + 90) % 180) - 90
	bottom_latitude = ((center[0] - (grid_length_in_km / 2 / km_per_degree_latitude) + 90) % 180) - 90

	# longitudes are between 0 and 180
	right_longitude = center[1] + (grid_length_in_km / 2 / km_per_degree_latitude) % 180
	left_longitude  = center[1] - (grid_length_in_km / 2 / km_per_degree_latitude) % 180

	# get distance between grid bounds
	dy = (top_latitude - bottom_latitude) / num_samples_per_row
	dx = ((right_longitude - left_longitude) % 180) / num_samples_per_row 
	print('dy = %f dx = %f' % (dy, dx))

	# loop over rows
	vertex_grid = []
	for i in range(0, num_samples_per_row):
		row_latitude = top_latitude - (i * dx)
		vertex_row = get_vertex_row((row_latitude, left_longitude), (row_latitude, right_longitude))
		vertex_grid.append(vertex_row)

	print(vertex_grid)
	return vertex_grid



get_vertex_grid((45,45))
