using UnityEngine;
using M = System.Math;

namespace ARLocation
{

    public class UnityLocationProvider : AbstractLocationProvider
    {
        private float androidMagneticDeclination;
        private AndroidNativeCompass androidNativeCompass;
        public override string Name => "UnityLocationProvider";

        public override bool IsCompassEnabled => Input.compass.enabled;

        protected override void RequestLocationAndCompassUpdates()
        {
            // Debug.Log("[UnityLocationProvider]: Requesting location updates...");

            Input.compass.enabled = true;

            Input.location.Start(
                (float)Options.AccuracyRadius,
                (float)Options.MinDistanceBetweenUpdates
            );
        }

        protected override void InnerOnEnabled()
        {
            androidMagneticDeclination = AndroidMagneticDeclination.GetDeclination(CurrentLocation.ToLocation());
            androidNativeCompass = new AndroidNativeCompass((float) (1.0  - LowPassFilterFactor));
        }

        protected override void UpdateLocationRequestStatus()
        {
            switch (Input.location.status)
            {
                case LocationServiceStatus.Initializing:
                    Status = LocationProviderStatus.Initializing;
                    break;

                case LocationServiceStatus.Failed:
                    Status = LocationProviderStatus.Failed;
                    break;

                case LocationServiceStatus.Running:
                    Status = LocationProviderStatus.Started;
                    break;

                case LocationServiceStatus.Stopped:
                    Status = LocationProviderStatus.Idle;
                    break;
            }
        }

        protected override LocationReading? ReadLocation()
        {
            if (!HasStarted)
            {
                return null;
            }

            var data = Input.location.lastData;

            if (ES3.Load<bool>("Test"))
            {
                return new LocationReading()
                {
                    //latitude = 35.1022391,
                    //longitude = 129.0615131,
                    latitude = 35.1024585,
                    longitude = 129.0615614,
                    altitude = data.altitude,
                    accuracy = data.horizontalAccuracy,
                    floor = -1,
                    timestamp = (long)(data.timestamp * 1000)
                };
            }
            else
            {
                return new LocationReading()
                {
                    latitude = data.latitude,
                    longitude = data.longitude,
                    altitude = data.altitude,
                    accuracy = data.horizontalAccuracy,
                    floor = -1,
                    timestamp = (long)(data.timestamp * 1000)
                };
            }
        }

        protected override HeadingReading? ReadHeading()
        {
            if (!HasStarted)
            {
                return null;
            }
            float magneticHeading = 0f;
            float trueHeading = 0f;
            if (ES3.Load<bool>("Test"))
            {
                // ReSharper disable once RedundantAssignment
                magneticHeading = Camera.main.transform.rotation.eulerAngles.y;

                // ReSharper disable once RedundantAssignment
                trueHeading = Camera.main.transform.rotation.eulerAngles.y;
            }
            else
            {
                // ReSharper disable once RedundantAssignment
                magneticHeading = Input.compass.magneticHeading;

                // ReSharper disable once RedundantAssignment
                trueHeading = Input.compass.trueHeading;
            }

         

#if PLATFORM_ANDROID
            var tiltCorrectedMagneticHeading = GetMagneticHeading();
            magneticHeading = tiltCorrectedMagneticHeading;
            trueHeading = tiltCorrectedMagneticHeading + androidMagneticDeclination;
#endif

            if (trueHeading < 0)
            {
                trueHeading += 360;
            }

            return new HeadingReading()
            {
                heading = trueHeading,
                magneticHeading = magneticHeading,
                accuracy = Input.compass.headingAccuracy,
                timestamp = (long)(Input.compass.timestamp * 1000),
                isMagneticHeadingAvailable = Input.compass.enabled
            };
        }

        private float GetMagneticHeading()
        {
#if PLATFORM_ANDROID
            if (!SystemInfo.supportsGyroscope || !ApplyCompassTiltCompensationOnAndroid || androidNativeCompass == null)
            {
                if (ES3.Load<bool>("Test"))
                {
                    return Camera.main.transform.rotation.eulerAngles.y;
                }
                else
                {
                    return Input.compass.magneticHeading;
                }
                //return Input.compass.magneticHeading;
            }

            return androidNativeCompass.GetMagneticHeading();

//            if (Screen.orientation == ScreenOrientation.Landscape)
//            {
//                return heading;// + 45;
//            }
//            else
//            {
//                return heading;
//            }

#else
            return Input.compass.magneticHeading;
#endif
        }
    }
}
