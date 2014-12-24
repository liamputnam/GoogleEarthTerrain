
// google maps elevation api url stem
var url_stem = 'https://maps.googleapis.com/maps/api/elevation/json';

// kilometers per degree of latitude (this is constant)
var km_per_degree_latitude = 111.2;

// number of kilometers across the side of the grid
var grid_length_in_km = 2;

// number of samples to take per row of the grid
var num_samples_per_row = 2;

// get the row of vertices, from start to end
function get_vertex_row(start, end)
{
	// construct url
	var path    = 'path=' + start[0] + ',' + start[1] + '|' + end[0] + ',' + end[1];
	var samples = 'samples=' + num_samples_per_row;
	var url = url_stem + '?' + path + '&' + samples;

	// fire it off
	var www : WWW = new WWW (url);

	// wait for request to complete 
	yield www;

	// and check for errors 
	if (www.error == null) 
	{ 
		// request completed! 
	} 
	else 
	{ 
		// something wrong! 
		Debug.Log("WWW Error: "+ www.error); 
	}

	var elevation_data = www.text;
	// probably have to parse the text into json...

	// loop over results
	var vertex_row = [];
	var result;
	for (result in elevation_data['results'])
	{
		// make a 3-tuple for each result (x,y,z)
		vertex_row[vertex_row.length] = [result['location']['lat'], result['location']['lng'], result['elevation']];
	}
	return vertex_row;

}


// get a 2d-grid of vertices around the center
function get_vertex_grid(center)
{

	// latitudes are between -90 and 90
	var top_latitude    = ((center[0] + (grid_length_in_km / 2 / km_per_degree_latitude) + 90) % 180) - 90;
	var bottom_latitude = ((center[0] - (grid_length_in_km / 2 / km_per_degree_latitude) + 90) % 180) - 90;

	// longitudes are between 0 and 180
	var right_longitude = center[1] + (grid_length_in_km / 2 / km_per_degree_latitude) % 180;
	var left_longitude  = center[1] - (grid_length_in_km / 2 / km_per_degree_latitude) % 180;

	// get distance between grid bounds
	var dy = (top_latitude - bottom_latitude) / num_samples_per_row;
	var dx = ((right_longitude - left_longitude) % 180) / num_samples_per_row;
	print('dy = %f dx = %f' % (dy, dx))

	// loop over rows
	var vertex_grid = [];
	
	for (var i = 0; i < num_samples_per_row; i++)
	{
		var row_latitude = top_latitude - (i * dx);
		var vertex_row   = get_vertex_row([row_latitude, left_longitude], [row_latitude, right_longitude]);
		vertex_grid[i]   = vertex_row;
	}
	//print(vertex_grid)
	return vertex_grid;
}


