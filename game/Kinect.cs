using Godot;
using System;
using System.Linq;
using static Godot.Mesh;

public partial class Kinect : Node3D
{
	private TextureRect _depthTextureRect;
	private Slider _depthMinSlider;
	private Slider _depthRangeSlider;
	private Slider _depthScaleSlider;

	private MeshInstance3D _meshInstance;

	private Kinect2 kinect;

	private uint _depthMin = 0;
	private uint _depthMax = 65536;
	private float _depthScale = 0.005f;

	override public void _Ready()
	{
		kinect = new Kinect2();
		kinect.InitSensor();

		_depthTextureRect = GetNode<TextureRect>("%DepthTextureRect");
		_depthMinSlider = GetNode<Slider>("%DepthMinSlider");
		_depthRangeSlider = GetNode<Slider>("%DepthRangeSlider");
		_depthScaleSlider = GetNode<Slider>("%DepthScaleSlider");
		_meshInstance = GetNode<MeshInstance3D>("%MeshInstance3D");

		_depthMinSlider.MaxValue = 65536;
		_depthRangeSlider.MaxValue = 65536;
		_depthScaleSlider.MaxValue = 100;

		_depthMinSlider.Value = _depthMin;
		_depthRangeSlider.Value = 65536;
		_depthScaleSlider.Value = _depthScale * 100000;

		_depthMinSlider.Connect("value_changed", new Callable(this, MethodName.OnDepthMinSliderValueChanged));
		_depthRangeSlider.Connect("value_changed", new Callable(this, MethodName.OnDepthRangeSliderValueChanged));
		_depthScaleSlider.Connect("value_changed", new Callable(this, MethodName.OnDepthScaleSliderValueChanged));

		// enable vr
		// var xrInterface = XRServer.FindInterface("OpenXR");
		// if (xrInterface != null && xrInterface.IsInitialized())
		// {
		// 	DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
		// 	GetViewport().UseXR = true;
		// }
	}

	private void OnDepthMinSliderValueChanged(float value)
	{
		_depthMin = (uint)value;
	}

	private void OnDepthRangeSliderValueChanged(float value)
	{
		_depthMax = _depthMin + (uint)value + 1;
	}

	private void OnDepthScaleSliderValueChanged(float value)
	{
		_depthScale = value * 0.00001f;
	}

	override public void _Process(double delta)
	{
		// Image depth = kinect.AcquireLatestDepthFrame(0, _depthMax, false);

		// if (depth != null && !depth.IsEmpty())
		// {
		// 	_depthTextureRect.Texture = ImageTexture.CreateFromImage(depth);
		// }

		var mesh = kinect.CreateMeshFromDepthFrame(0, _depthMax);
		if (mesh != null)
		{
			if (_meshInstance.Mesh != null)
			{
				_meshInstance.Mesh.Dispose();
			}
			_meshInstance.Mesh = mesh;
		}

		Image color = kinect.AcquireLatestColorFrame();

		if (color != null && !color.IsEmpty())
		{
			var mat = _meshInstance.MaterialOverride as StandardMaterial3D;
			if (mat.AlbedoTexture != null)
				mat.AlbedoTexture.Dispose();
			mat.AlbedoTexture = ImageTexture.CreateFromImage(color);
		}

		color.Dispose();
	}

}
