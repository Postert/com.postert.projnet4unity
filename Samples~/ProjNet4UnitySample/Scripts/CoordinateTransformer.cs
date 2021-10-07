using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Transformes 
/// </summary>
public class CoordinateTransformer : MonoBehaviour
{
    /// <summary>
    /// Specifies the zone of the ETRS/UTM coordinate system (between 1 and 60).
    /// <remarks>The default ETRS/UTM coordinate system is 32 which is most suitable for Hamburg in Germany.</remarks>
    /// </summary>
    [SerializeField, Range(1, 60)]
    private int UTMZone = 32;

    /// <summary>
    /// Hemisphere of the region. Choose between the northern or southern hemisphere.
    /// <remarks>The default hemisphere setting is Northern as it is suitable for Hamburg in Germany.</remarks>
    /// </summary>
    [SerializeField]
    private Hemispheres Hemisphere = Hemispheres.Northern;

    private enum Hemispheres
    {
        Northern,
        Southern
    }

    /// <summary>
    /// Returns true if the zone is on the northern hemisphere. Otherwise it returns false. 
    /// </summary>
    /// <returns>The default value is true.</returns>
    private bool IsNorthernHemisphere()
    {
        return Hemisphere switch
        {
            Hemispheres.Northern => true,
            Hemispheres.Southern => false,
            _ => true,
        };
    }

    /// <summary>
    /// The ETRS/UTM coordinates of an anchor point. Make sure that the anchor point is within a distance smaller than 100 km to the anchor for each axis.
    /// <remarks>The default value of the height component (z) is 0. If the anchor was derived from geographic coordinates z is 0 as well.</remarks>
    /// </summary>
    [SerializeField]
    private double3 UTMAnchorCoordinates = new double3(567475, 5932475, 0);


    /// <summary>
    /// The terrain from which the coordinates' height is derived from. Make sure that the coordinates are within the expension of the terrain.
    /// <remarks>Required for the use of the method <see cref="GetTerrainHeight(double2)"/></remarks>
    /// </summary>
    [SerializeReference]
    private Terrain Terrain;


    /// <summary>
    /// Use <see cref="CoordinateSystemServices"/> instead.
    /// </summary>
    private CoordinateSystemServices _CoordinateSystemServices;

    /// <summary>
    /// Contains the reference to a CoordinateSystemService.
    /// </summary>
    private CoordinateSystemServices CoordinateSystemServices
    {
        get
        {
            if (_CoordinateSystemServices == null)
            {
                _CoordinateSystemServices = new CoordinateSystemServices();
            }
            return _CoordinateSystemServices;
        }
    }


    /// <summary>
    /// Use <see cref="UTMToGeographicCoordinates"/> instead.
    /// </summary>
    private ICoordinateTransformation _UTMToGeographicCoordinates;

    /// <summary>
    /// Creates a coordinate transformation from the ETRS/UTM coordinate system to the geographic coordinate system. 
    /// <remarks>The ETRS/UTM coordinate system's <see cref="UTMZone"/>, <see cref="Hemisphere"/>, and <see cref="UTMAnchorCoordinates"/> have to be predefined before generating the transformation.</remarks>
    /// </summary>
    private ICoordinateTransformation UTMToGeographicCoordinates
    {
        get
        {
            if (_UTMToGeographicCoordinates == null)
            {
                _UTMToGeographicCoordinates = CoordinateSystemServices.CreateTransformation(ProjectedCoordinateSystem.WGS84_UTM(UTMZone, IsNorthernHemisphere()), GeographicCoordinateSystem.WGS84);
            }
            return _UTMToGeographicCoordinates;
        }
    }

    /// <summary>
    /// Use <see cref="GeographicToUTMCoordinates"/> instead.
    /// </summary>
    private ICoordinateTransformation _GeographicToUTMCoordinates;

    /// <summary>
    /// Creates a coordinate transformation from the geographic coordinate system to the ETRS/UTM coordinate system. 
    /// <remarks>The ETRS/UTM coordinate system's <see cref="UTMZone"/>, <see cref="Hemisphere"/>, and <see cref="UTMAnchorCoordinates"/> have to be predefined before generating the transformation.</remarks>
    /// </summary>
    private ICoordinateTransformation GeographicToUTMCoordinates
    {
        get
        {
            if (_GeographicToUTMCoordinates == null)
            {
                _GeographicToUTMCoordinates = CoordinateSystemServices.CreateTransformation(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WGS84_UTM(UTMZone, IsNorthernHemisphere()));
            }
            return _GeographicToUTMCoordinates;
        }
    }

    /// <summary>
    /// Converts 2D ETRS/UTM coordinates to geographic coordinates. 
    /// </summary>
    /// <param name="utmCoordinates">ETRS/UTM coordinates without height information.</param>
    /// <returns>Geographic coordinates.</returns>
    public (double longitude, double latitude) ConvertToLatitudeLongitude(double2 utmCoordinates)
    {
        double[] coords = UTMToGeographicCoordinates.MathTransform.Transform(new double[] { utmCoordinates.x, utmCoordinates.y });
        return (coords[0], coords[1]);
    }


    /// <summary>
    /// Converts 3D Unity coordinates to a geographic coordinates. 
    /// </summary>
    /// <param name="unityCoordinates">Unity coordinates without height information.</param>
    /// <returns>Geographic coordinates.</returns>
    public (double longitude, double latitude) ConvertToLatitudeLongitude(Vector3 unityCoordinates)
    {
        double3 utmCoordinates = GetUTMCoordinates(unityCoordinates);
        return ConvertToLatitudeLongitude(new double2(utmCoordinates.x, utmCoordinates.y));
    }


    /// <summary>
    /// Converts geographic coordinates to ETRS/UTM coordinates. 
    /// </summary>
    /// <param name="geographicCoordinates">Geographic coordinates as a tuple (latitude, longitude).</param>
    /// <returns>ETRS/UTM coordinates.</returns>
    public double2 ConvertToUTM((double longitude, double latitude) geographicCoordinates)
    {
        double[] coords = GeographicToUTMCoordinates.MathTransform.Transform(new double[] { geographicCoordinates.longitude, geographicCoordinates.latitude });
        return (new double2(coords[0], coords[1]));
    }


    /// <summary>
    /// Converts 2D Unity coordinates to ETRS/UTM coordinates.
    /// </summary>
    /// <param name="unityCoordinates">Unity coordinates without height information.</param>
    /// <returns>ETRS/UTM coordinates without height information.</returns>
    public double2 ConvertToUTM(Vector2 unityCoordinates)
    {
        double3 utmCoordinates = GetUTMCoordinates(unityCoordinates);
        return new double2(utmCoordinates.x, utmCoordinates.y);
    }

    /// <summary>
    /// Converts 3D Unity coordinates to ETRS/UTM coordinates. 
    /// </summary>
    /// <param name="unityCoordinates">Unity coordinates with height information.</param>
    /// <returns>ETRS/UTM coordinates with height information.</returns>
    public double3 ConvertToUTM(Vector3 unityCoordinates)
    {
        return GetUTMCoordinates(unityCoordinates);
    }

    /// <summary>
    /// Converts 2D geographic coordinates to 3D Unity coordinates. The <see cref="Terrain"/>'s height in the given coordinates position. By default the height is 0 if no <see cref="Terrain"/> is associated.
    /// </summary>
    /// <param name="geographicCoordinates">Geographic coordinates.</param>
    /// <returns>ETRS/UTM coordinates with height derived from the terrain or 0.</returns>
    public Vector3 ConvertToUnity((double longitude, double latitude) geographicCoordinates)
    {
        double2 utmCoordinates = ConvertToUTM(geographicCoordinates);
        return ConvertToUnity(utmCoordinates);
    }


    /// <summary>
    /// Converts 2D ETRS/UTM coordinates to 3D Unity coordinates. The <see cref="Terrain"/>'s height in the given coordinates position. By default the height is 0 if no Terrain is associated.
    /// </summary>
    /// <param name="utmCoordinates">Geographic coordinates.</param>
    /// <returns>ETRS/UTM coordinates.</returns>
    public Vector3 ConvertToUnity(double2 utmCoordinates)
    {
        float? terrainHeight = GetTerrainHeight(utmCoordinates);

        if (terrainHeight.HasValue)
        {
            return GetUnityCoordinates(new double3(utmCoordinates.x, utmCoordinates.y, terrainHeight.Value));
        }
        else
        {
            Debug.LogWarningFormat("No terrain provided. Thus the requested Unity coordinate has a default hight component of 0.");
            return GetUnityCoordinates(new double3(utmCoordinates.x, utmCoordinates.y, 0));
        }
    }


    /// <summary>
    /// Converts 3D ETRS/UTM coordinates to 3D Unity coordinates. The <see cref="Terrain"/>'s height in the given coordinates position. By default the height is 0 if no Terrain is associated.
    /// </summary>
    /// <param name="utmCoordinates">Geographic coordinates.</param>
    /// <returns>ETRS/UTM coordinates.</returns>
    public Vector3 ConvertToUnity(double3 utmCoordinates)
    {
        return GetUnityCoordinates(utmCoordinates);
    }

    /// <summary>
    /// Calculates the associated <see cref="Terrain"/>'s height. The returned value is the global y-(height)-value in Unity. Objects placed in there with the calculated height are placed exactly on the terrain. 
    /// </summary>
    /// <param name="pointsUTMCoordinate"></param>
    /// <returns>Height of the terrain in the given position or null if no terrain is associated.</returns>
    public float? GetTerrainHeight(double2 pointsUTMCoordinate)
    {
        if (Terrain == null)
        {
            //throw new ArgumentNullException("There was no terain to calculate the object height.");
            return null;
        }

        Vector3 unity2DCoordinate = GetUnityCoordinates(new double3(pointsUTMCoordinate.x, pointsUTMCoordinate.y, 0));
        float terrainHeightInTerrainGameObject = Terrain.SampleHeight(unity2DCoordinate);

        float heightOfTerrainGameObject = Terrain.gameObject.transform.position.y;

        return terrainHeightInTerrainGameObject + heightOfTerrainGameObject;
    }


    /// <summary>
    /// Calculates the position of the point relative to the specified anchor point and returns left-handed coordinate values.
    /// </summary>
    /// <remarks>
    /// Use this method to calculate the coordinates based on an anchor point, as Unity restricts the coordinates to float values, which excludes the direct processing of ETRS/UTM coordinates in Unity3D. 
    /// </remarks>
    /// <param name="pointsUTMCoordinate">The ETRS/UTM coordinates of a point to be displayed in Unity.</param>
    /// <returns>Returns coordinate values that can be processed within the Unity3D's scene, which are already converted to Unity3D's left-handed coordinate system.</returns>
    /// <exception cref="System.ArgumentException">Thrown when a coordinate component exceeds a distance of 100 km on one of the axes. The latter exceeds the capacity of a float.</exception>
    private Vector3 GetUnityCoordinates(double3 pointsUTMCoordinate)
    {
        float3 positionRelativeToAnchorPoint = new float3();
        for (int i = 0; i < 3; i++)
        {
            /// Calculate the possition relative to the <seealso cref="UTMAnchorCoordinates"/> and check if the calculated coordinate component value can be processed by Unity.
            double coordinateComponent = pointsUTMCoordinate[i] - UTMAnchorCoordinates[i];
            if (coordinateComponent < 100 || coordinateComponent * -1 < 100)
            {
                positionRelativeToAnchorPoint[i] = (float)coordinateComponent;
            }
            else
            {
                throw new ArgumentException("The point to be converted must not exceed a distance of 100 km from the anchor point on any axis. pointsUTMCoordinate value: " + pointsUTMCoordinate[i] + ", anchorsUTMCoordinate value: " + pointsUTMCoordinate[i] + ", position relative to anchor point on this axis: " + positionRelativeToAnchorPoint[i]);
            }
        }

        return SwitchLeftHandedRightHandedCoordinates((Vector3)positionRelativeToAnchorPoint);
    }


    /// <summary>
    /// Calculates the ETRS/UTM coordinates from a given Unity3D coordinate and returns right-handed coordinate values.
    /// </summary>
    /// <remarks>
    /// Use this method to calculate the geo-coordinates based on the predefined <seealso cref="UTMAnchorCoordinates"/> anchor point. 
    /// </remarks>
    /// <param name="pointsUnityCoordinate">The Unity3D coordinates of a point to be displayed in Unity.</param>
    /// <returns>Returns ETRS/UTM coordinate values which are already converted to a standard right-handed coordinate system.</returns>
    private double3 GetUTMCoordinates(Vector3 pointsUnityCoordinate)
    {
        Vector3 pointsSwitchedUnityCoordinate = SwitchLeftHandedRightHandedCoordinates(pointsUnityCoordinate);

        double3 utmCoordinates = new double3();
        for (int i = 0; i < 3; i++)
        {
            double coordinateComponent = pointsSwitchedUnityCoordinate[i] + UTMAnchorCoordinates[i];

            utmCoordinates[i] = (float)coordinateComponent;
        }

        return utmCoordinates;
    }


    /// <summary>
    /// Switches coordinates left or right-handed coordinate system values to the other system.
    /// </summary>
    /// <param name="coordinateValues">Coordinate values to be converted.</param>
    /// <returns>Converted coordinate values.</returns>
    private Vector3 SwitchLeftHandedRightHandedCoordinates(Vector3 coordinateValues)
    {
        return new Vector3(coordinateValues.x, coordinateValues.z, coordinateValues.y);
    }

    /// <summary>
    /// Switches coordinates left or right-handed coordinate system values to the other system.
    /// </summary>
    /// <param name="coordinateValues">Coordinate values to be converted.</param>
    /// <returns>Converted coordinate values.</returns>
    private double3 SwitchLeftHandedRightHandedCoordinates(double3 coordinateValues)
    {
        return new double3(coordinateValues.x, coordinateValues.z, coordinateValues.y);
    }

    public static CoordinateTransformer Instance;

    private void Awake()
    {
        // If Instance is not null (any time after the first time)
        // AND
        // If Instance is not 'this' (after the first time)
        if (Instance != null && Instance != this)
        {
            // ...then destroy the game object this script component is attached to.
            Destroy(gameObject);
            Debug.LogWarningFormat("Only one CoordinateTransformer can exist.");
        }
        else
        {
            // Tell Unity not to destory the GameObject this
            //  is attached to between scenes.
            DontDestroyOnLoad(gameObject);
            // Save an internal reference to the first instance of this class
            Instance = this;
        }
    }
}