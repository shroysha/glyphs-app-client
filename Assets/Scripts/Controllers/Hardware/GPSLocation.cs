using System;
using UnityEngine;

[Serializable]
public struct GPSLocation {

	public static readonly GPSLocation UNDEFINED = new GPSLocation(Double.NaN, Double.NaN, double.NaN);

	private static readonly double DEFAULT_ALTITUDE = 0.0f;

	/*
	 * If your displacements aren't too great (less than a few kilometers) and you're not right at the poles, 
	 * use the quick and dirty estimate that 111,111 meters (111.111 km) in the y direction is 1 
	 * degree (of latitude) and 111,111 * cos(latitude) meters in the x direction is 1 degree (of longitude).
	 */
	private static readonly double LAT_TO_M_SCALAR = 111111;
	private static readonly double LONG_TO_M_SCALAR = 111111;

	// Semi-axes of WGS-84 geoidal reference
	private static readonly double WGS84_a = 6378137.0;  // Major semiaxis [m]
	private static readonly double WGS84_b = 6356752.3;  // Minor semiaxis [m]

	// degrees to radians
	public static double deg2rad(double degrees) {
		return Math.PI * degrees / 180.0;
	}

	// radians to degrees
	public static double rad2deg(double radians) {
		return 180.0 * radians / Math.PI;
	}

	//Earth radius at a given latitude, according to the WGS-84 ellipsoid [m]
	public static double WGS84EarthRadius(double lat) {
		// http://en.wikipedia.org/wiki/Earth_radius
		double An = WGS84_a*WGS84_a * Math.Cos(lat);
		double Bn = WGS84_b*WGS84_b * Math.Sin(lat);
		double Ad = WGS84_a * Math.Cos(lat);
		double Bd = WGS84_b * Math.Sin(lat);

		return Math.Sqrt( (An*An + Bn*Bn)/(Ad*Ad + Bd*Bd) );
	}
		
	public double latitude;
	public double longitude;
	public double altitude;

	public GPSLocation (double latitude, double longitude) {
		this.latitude = latitude;
		this.longitude = longitude;
		this.altitude = DEFAULT_ALTITUDE;
	}

	public GPSLocation (double latitude, double longitude, double altitude) {
		this.latitude = latitude;
		this.longitude = longitude;
		this.altitude = altitude;
	}

	public GPSLocation[] calculateLatLongBoundingBox(double halfDistanceInM) {

		double latRad = deg2rad(latitude);
		double lonRad = deg2rad(longitude);

		// Radius of Earth at given latitude
		double radius = WGS84EarthRadius(latRad);
		// Radius of the parallel at given latitude
		double pradius = radius * Math.Cos(latRad);

		double minLatRad = latRad - (halfDistanceInM / radius);
		double minLonRad = lonRad - (halfDistanceInM / pradius);
		double maxLatRad = latRad + (halfDistanceInM / radius);
		double maxLonRad = lonRad + (halfDistanceInM / pradius);

		double minLat = rad2deg (minLatRad);
		double minLon = rad2deg (minLonRad);
		double maxLat = rad2deg (maxLatRad);
		double maxLon = rad2deg (maxLonRad);


		GPSLocation minBound = new GPSLocation(minLat, minLon);
		GPSLocation maxBound = new GPSLocation(maxLat, maxLon);

		GPSLocation[] bounds = new GPSLocation[2];
		bounds [0] = minBound;
		bounds [1] = maxBound;

		return bounds;
	}

	public bool isInsideBounds(GPSLocation[] bounds) {
		GPSLocation minBound = bounds[0];
		GPSLocation maxBound = bounds[1];

		bool greaterThanMinBounds = minBound.latitude < this.latitude && minBound.longitude < this.longitude;
		bool lesserThanMaxBounds = this.latitude < maxBound.latitude && this.longitude < maxBound.longitude;

		return greaterThanMinBounds && lesserThanMaxBounds;
	}

	public Vector2 calculatePolarizedOffsetInMeters(GPSLocation otherLocation) {

		double latitudeDifference = otherLocation.latitude - this.latitude;
		double longitudeDifference = otherLocation.longitude - this.longitude;

		double latitudeInM = latitudeDifference * LAT_TO_M_SCALAR;
		double longitudeInM = (longitudeDifference / -Math.Cos (this.latitude)) * LONG_TO_M_SCALAR;
	
		return new Vector2 ((float)latitudeInM, (float)longitudeInM);
	}

	public float calculateDistanceToLocationInM(GPSLocation otherLocation) {
		/*
            The Haversine formula according to Dr. Math.
            http://mathforum.org/library/drmath/view/51879.html
                
            dlon = lon2 - lon1
            dlat = lat2 - lat1
            a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            d = R * c
                
            Where
                * dlon is the change in longitude
                * dlat is the change in latitude
                * c is the great circle distance in Radians.
                * R is the radius of a spherical Earth.
                * The locations of the two points in 
                    spherical coordinates (longitude and 
                    latitude) are lon1,lat1 and lon2, lat2.
    	*/
		double kEarthRadiusKms = 6376.5;


		double dLat1InRad = deg2rad(this.latitude);
		double dLong1InRad = deg2rad(this.longitude);
		double dLat2InRad = deg2rad(otherLocation.latitude);
		double dLong2InRad = deg2rad(otherLocation.longitude);

		double dLongitude = dLong2InRad - dLong1InRad;
		double dLatitude = dLat2InRad - dLat1InRad;

		// Intermediate result a.
		double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) + 
			Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * 
			Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

		// Intermediate result c (great circle distance in Radians).
		double c = 2.0 * Math.Asin(Math.Sqrt(a));

		// Distance.
		// const Double kEarthRadiusMiles = 3956.0;
		double dDistance = kEarthRadiusKms * c * 1000.0; // 1000 meters in kilometer

		return (float)dDistance;
	}

	public float calculateBearingToLocation(GPSLocation otherLocation) {
		double lat1 = deg2rad (this.latitude);
		double lon1 = deg2rad (this.longitude);
		double lat2 = deg2rad (otherLocation.latitude);
		double lon2 = deg2rad (otherLocation.longitude);

		double dLon = lon1 - lon2;

		double y = Math.Sin(dLon) * Math.Cos(lat2);
		double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

		double brng = Math.Atan2(y, x);
		double bearingDegrees = (-rad2deg (brng) + 360.0) % 360.0;

		return (float) bearingDegrees;
	}
		
	public override string ToString() {
		return "" + this.latitude + ","  + this.longitude;
	}

	public override bool Equals(System.Object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is GPSLocation) {
			GPSLocation other = (GPSLocation)obj;

			return this.latitude == other.latitude && this.longitude == other.longitude;
		}

		return false;
	}

}


