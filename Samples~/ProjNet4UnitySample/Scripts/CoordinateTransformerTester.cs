using Unity.Mathematics;
using UnityEngine;

public class CoordinateTransformerTester : MonoBehaviour
{
    private CoordinateTransformer CoordinateTransformer;

    [SerializeField]
    private GameObject Marker;

    // Start is called before the first frame update
    private void Start()
    {
        CoordinateTransformer = GameObject.FindObjectOfType<CoordinateTransformer>();

        UnitTest();
    }


    private void UnitTest()
    {
        (double longitude, double latitude) geographicCoordinatesStoltenpark;
        geographicCoordinatesStoltenpark.longitude = 10.028691;
        geographicCoordinatesStoltenpark.latitude = 53.551218;

        // Stoltenpark with given geographic coordinates

        Debug.LogFormat("The original given geographic coordinates:\nlongitude: {0}, latitude: {1}", geographicCoordinatesStoltenpark.longitude, geographicCoordinatesStoltenpark.latitude);

        double2 utmCoordinatesStoltenparkOfGeographicCoordinates = CoordinateTransformer.ConvertToUTM(geographicCoordinatesStoltenpark);
        Debug.LogFormat("The utm coordinates of the Stoltenpark:\nx: {0}, y: {1}", utmCoordinatesStoltenparkOfGeographicCoordinates.x, utmCoordinatesStoltenparkOfGeographicCoordinates.y);

        Vector3 unityCoordinatesStoltenparkOfGeographicCoordinates = CoordinateTransformer.ConvertToUnity(geographicCoordinatesStoltenpark);
        Debug.LogFormat("The unity coordinates of the Stoltenpark:\nx: {0}, y: {1}, z:{2}", unityCoordinatesStoltenparkOfGeographicCoordinates.x, unityCoordinatesStoltenparkOfGeographicCoordinates.y, unityCoordinatesStoltenparkOfGeographicCoordinates.z);

        GameObject marker = Instantiate(Marker, unityCoordinatesStoltenparkOfGeographicCoordinates, Quaternion.identity);

        Debug.LogFormat("The position of the marker in Unity instantiated with the mentioned coordinates:\nx: {0}, y: {1}, z:{2}", marker.transform.transform.position.x, marker.transform.transform.position.y, marker.transform.transform.position.z);

        Debug.LogFormat("Geographic coordinates of marker from Unity coordinates: {0}", CoordinateTransformer.ConvertToLatitudeLongitude(marker.transform.transform.position));
        Debug.LogFormat("Geographic coordinates of marker from UTM coordinates without Unity conversion (and loss of precision): {0}", CoordinateTransformer.ConvertToLatitudeLongitude(utmCoordinatesStoltenparkOfGeographicCoordinates));
    }
}