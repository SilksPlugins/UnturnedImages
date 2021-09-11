using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnturnedImages.Module.Images
{
    public class CustomVehicleTool : MonoBehaviour
	{
        public class CustomVehicleIconInfo
        {
			public VehicleAsset VehicleAsset { get; }

            public string OutputPath { get; }

			public int Width { get; }

			public int Height { get; }

            public CustomVehicleIconInfo(VehicleAsset vehicleAsset, string outputPath, int width, int height)
            {
                VehicleAsset = vehicleAsset;
                OutputPath = outputPath;
                Width = width;
                Height = height;
            }
        }

        private static CustomVehicleTool? _instance;

        private static readonly Queue<CustomVehicleIconInfo> Icons = new();

        public static void Load()
        {
            _instance = UnturnedImagesModule.Instance!.GameObject!.AddComponent<CustomVehicleTool>();
        }

        public static void Unload()
        {
            Destroy(_instance);

            _instance = null;
        }


        public static Transform? GetVehicle(VehicleAsset vehicleAsset)
		{
			var gameObject = vehicleAsset.model?.getOrLoad();

            if (gameObject == null)
            {
                return null;
			}

            var transform = Instantiate(gameObject).transform;

            transform.name = vehicleAsset.id.ToString();

            return transform;

        }
		
		public static void QueueVehicleIcon(VehicleAsset vehicleAsset, string outputPath, int width, int height)
		{
			var vehicleIconInfo = new CustomVehicleIconInfo(vehicleAsset, outputPath, width, height);

			Icons.Enqueue(vehicleIconInfo);
		}
		
		private void Update()
		{
			if (Icons.Count == 0)
			{
				return;
			}

			var vehicleIconInfo = Icons.Dequeue();

            var vehicleAsset = vehicleIconInfo.VehicleAsset;
            var vehicle = GetVehicle(vehicleAsset);

            if (vehicle == null)
            {
                UnturnedLog.error($"Could not get model for vehicle with ID {vehicleAsset.id}");
                return;
            }

            Layerer.relayer(vehicle, LayerMasks.VEHICLE);

            var weirdLookingObjects = new[]
            {
                "DepthMask"
            };

            foreach (Transform child in vehicle)
            {
                if (weirdLookingObjects.Contains(child.name))
                {
                    child.gameObject.SetActive(false);
                }
            }

            var vehicleParent = new GameObject().transform;
            vehicle.SetParent(vehicleParent);

            vehicleParent.position = new Vector3(-256f, -256f, 0f);

            var cameraTransform = new GameObject().transform;

            cameraTransform.SetParent(vehicle, false);

            vehicle.Rotate(10, 135, -10);
            cameraTransform.rotation = Quaternion.identity;

            var orthographicSize = CustomImageTool.CalculateOrthographicSize(vehicleAsset, vehicleParent.gameObject,
                cameraTransform, vehicleIconInfo.Width, vehicleIconInfo.Height, out var cameraPosition);

            cameraTransform.position = cameraPosition;

            Texture2D texture = CustomImageTool.CaptureIcon(vehicleAsset.id, 0, vehicle, cameraTransform,
                vehicleIconInfo.Width, vehicleIconInfo.Height, orthographicSize, true);
            
            var path = $"{vehicleIconInfo.OutputPath}.png";

            var bytes = texture.EncodeToPNG();

            UnturnedLog.info(path);
            ReadWrite.writeBytes(path, false, false, bytes);

            Destroy(vehicleParent.gameObject);
        }
	}
}
