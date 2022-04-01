using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnturnedImages.Module.Images
{
    public class CustomImageTool : MonoBehaviour
    {
        private static CustomImageTool? _tool;

        private Camera? _cameraComponent;
        private Light? _lightComponent;

        private readonly List<Renderer> _renderers = new();

		public static void Load()
		{
            _tool = UnturnedImagesModule.Instance!.GameObject!.AddComponent<CustomImageTool>();
		}

		public static void Unload()
        {
            Destroy(_tool);

            _tool = null;
        }

		private void Start()
        {
            _cameraComponent = gameObject.AddComponent<Camera>();
            _cameraComponent.orthographic = true;
			_cameraComponent.enabled = false;

            _lightComponent = gameObject.AddComponent<Light>();
            _lightComponent.enabled = false;
        }

		public static Texture2D CaptureIcon(ushort id, ushort skin, Transform model, Transform icon, int width, int height, float orthoSize, bool readableOnCPU)
		{
			if (_tool == null)
            {
                throw new Exception("No instance of CustomImageTool");
			}

			if (_tool._cameraComponent == null)
            {
                throw new Exception("No instance of camera");
			}

			if (_tool._lightComponent == null)
            {
                throw new Exception("No instance of light");
            }

			_tool.transform.position = icon.position;
			_tool.transform.rotation = icon.rotation;

            const int antiAliasing = 4;
			var temporary = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, antiAliasing);
			temporary.name = $"Render_{id}_{skin}";

			RenderTexture.active = temporary;
			_tool._cameraComponent.targetTexture = temporary;
			_tool._cameraComponent.orthographicSize = orthoSize;
			var fog = RenderSettings.fog;
			var ambientMode = RenderSettings.ambientMode;
			var ambientSkyColor = RenderSettings.ambientSkyColor;
			var ambientEquatorColor = RenderSettings.ambientEquatorColor;
			var ambientGroundColor = RenderSettings.ambientGroundColor;

			RenderSettings.fog = false;
			RenderSettings.ambientMode = AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = Color.white;
			RenderSettings.ambientEquatorColor = Color.white;
			RenderSettings.ambientGroundColor = Color.white;

			_tool._lightComponent.enabled = true;
			GL.Clear(true, true, ColorEx.BlackZeroAlpha);
            _tool._cameraComponent.cullingMask = 67313664;
			_tool._cameraComponent.farClipPlane = 128f;
			_tool._cameraComponent.clearFlags = CameraClearFlags.Nothing;
			_tool._cameraComponent.Render();
			_tool._lightComponent.enabled = false;

            RenderSettings.fog = fog;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientSkyColor = ambientSkyColor;
			RenderSettings.ambientEquatorColor = ambientEquatorColor;
			RenderSettings.ambientGroundColor = ambientGroundColor;

			model.position = new Vector3(0f, -256f, -256f);

			Destroy(model.gameObject);

            var texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                name = $"Icon_{id}_{skin}",
                filterMode = FilterMode.Point
            };

            texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			texture2D.Apply(false, !readableOnCPU);
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}

        public static float CalculateOrthographicSize(VehicleAsset assetContext, GameObject modelGameObject,
            Transform cameraTransform, int renderWidth, int renderHeight, out Vector3 cameraPosition)
		{
            if (_tool == null)
            {
                throw new Exception("No instance of CustomImageTool");
            }

            return _tool.CalculateOrthographicSizeInternal(assetContext, modelGameObject, cameraTransform, renderWidth,
                renderHeight, out cameraPosition);
        }

		// From SDG.Unturned.ItemTool
        private float CalculateOrthographicSizeInternal(VehicleAsset assetContext, GameObject modelGameObject,
            Transform cameraTransform, int renderWidth, int renderHeight, out Vector3 cameraPosition)
        {
            cameraPosition = cameraTransform.position;

            if (_cameraComponent == null)
            {
                throw new Exception("No instance of camera");
            }

			_renderers.Clear();
			modelGameObject.GetComponentsInChildren(false, _renderers);
			var bounds = default(Bounds);
			var flag = false;

            foreach (Renderer renderer in _renderers)
			{
				if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
				{
					if (flag)
					{
						bounds.Encapsulate(renderer.bounds);
					}
					else
					{
						flag = true;
						bounds = renderer.bounds;
					}
				}
			}

			if (!flag)
			{
				return 1f;
			}

			var extents = bounds.extents;
			if (extents.ContainsInfinity() || extents.ContainsNaN() || extents.IsNearlyZero(0.001f))
			{
				Assets.reportError(assetContext, "has invalid icon world extent ({0})", extents);
				return 1f;
			}

			var bounds2 = new Bounds(cameraTransform.InverseTransformVector(extents), Vector3.zero);
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(-extents));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-extents.x, extents.y, extents.z)));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(extents.x, -extents.y, extents.z)));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(extents.x, extents.y, -extents.z)));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-extents.x, -extents.y, extents.z)));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(-extents.x, extents.y, -extents.z)));
			bounds2.Encapsulate(cameraTransform.InverseTransformVector(new Vector3(extents.x, -extents.y, -extents.z)));
			var extents2 = bounds2.extents;

			if (extents2.ContainsInfinity() || extents2.ContainsNaN() || extents2.IsNearlyZero(0.001f))
			{
				Assets.reportError(assetContext, "has invalid icon local extent ({0})", extents);
				return 1f;
			}

			var absX = Mathf.Abs(extents2.x);
			var absY = Mathf.Abs(extents2.y);
			var absZ = Mathf.Abs(extents2.z);

			var nearClipPlane = _cameraComponent.nearClipPlane;
			cameraPosition = bounds.center - cameraTransform.forward * (absZ + 0.02f + nearClipPlane);

			absX *= (renderWidth + 16) / (float)renderWidth;
			absY *= (renderHeight + 16) / (float)renderHeight;

			var num4 = (float)renderWidth / renderHeight;
			var num5 = absX / absY;
			var num6 = (num5 > num4) ? (num5 / num4) : 1f;

			return absY * num6;
		}
	}
}
