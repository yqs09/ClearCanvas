#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.Volumes
{
	/// <summary>
	/// Represents a 3-dimensional raster volume.
	/// </summary>
	/// <remarks>
	/// The <see cref="Volume"/> class encapsulates 3-dimensional raster data (i.e. a volume defined by a block of voxels).
	/// Typically, an instance of <see cref="Volume"/> is created from a set of DICOM images by calling
	/// <see cref="Create(IDisplaySet)"/> or any of its overloads. Alternatively, the <see cref="VolumeCache"/>
	/// may be used to obtain a wrapper object that allows access to a shared, cached and memory-managed
	/// instance of <see cref="Volume"/>.
	/// </remarks>
	/// <seealso cref="U16Volume"/>
	/// <seealso cref="S16Volume"/>
	/// <seealso cref="U8Volume"/>
	/// <seealso cref="S8Volume"/>
	public abstract partial class Volume : IDisposable
	{
		#region Private fields

		private readonly VolumeHeader _volumeHeader;

		private int? _minVolumeValue;
		private int? _maxVolumeValue;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes the <see cref="Volume"/>.
		/// </summary>
		/// <param name="volumeHeader"></param>
		/// <param name="minVolumeValue"></param>
		/// <param name="maxVolumeValue"></param>
		internal Volume(VolumeHeader volumeHeader, int? minVolumeValue, int? maxVolumeValue)
		{
			Platform.CheckForNullReference(volumeHeader, "volumeHeader");

			_volumeHeader = volumeHeader;
			_minVolumeValue = minVolumeValue;
			_maxVolumeValue = maxVolumeValue;
		}

		#endregion

		#region Indexers

		/// <summary>
		/// Gets the value in the volume data at the specified array index.
		/// </summary>
		public int this[int i]
		{
			get { return GetArrayValue(i); }
		}

		/// <summary>
		/// Gets the value in the volume data at the specified array indices.
		/// </summary>
		public int this[int x, int y, int z]
		{
			get
			{
				const string message = "Specified {0}-index exceeds array bounds.";
				var arrayDimensions = ArrayDimensions;
				if (!(x >= 0 && x < arrayDimensions.Width))
					throw new ArgumentOutOfRangeException("x", x, string.Format(message, "X"));
				else if (!(y >= 0 && y < arrayDimensions.Height))
					throw new ArgumentOutOfRangeException("y", y, string.Format(message, "Y"));
				else if (!(z >= 0 && z < arrayDimensions.Depth))
					throw new ArgumentOutOfRangeException("z", z, string.Format(message, "Z"));
				return GetArrayValue(x + arrayDimensions.Width*(y + arrayDimensions.Height*z));
			}
		}

		/// <summary>
		/// Called to get the value in the volume data at the specified array index.
		/// </summary>
		protected abstract int GetArrayValue(int i);

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the array containing the volume data.
		/// </summary>
		public abstract Array Array { get; }

		/// <summary>
		/// Gets the total number of voxels in the volume.
		/// </summary>
		public int ArrayLength
		{
			get { return Array.Length; }
		}

		/// <summary>
		/// Gets the dimensions of the volume data.
		/// </summary>
		public Size3D ArrayDimensions
		{
			get { return _volumeHeader.ArrayDimensions; }
		}

		/// <summary>
		/// Gets whether or not the values of the volume data are signed.
		/// </summary>
		public abstract bool Signed { get; }

		/// <summary>
		/// Gets the number of bits per voxel of the volume data.
		/// </summary>
		public abstract int BitsPerVoxel { get; }

		/// <summary>
		/// Gets the effective size of the volume (that is, the <see cref="ArrayDimensions"/> multiplied by the <see cref="VoxelSpacing"/> in each respective dimension).
		/// </summary>
		public Vector3D VolumeSize
		{
			get { return _volumeHeader.VolumeSize; }
		}

		/// <summary>
		/// Gets the effective bounds of the volume (that is, the 3-dimensional region occupied by the volume after accounting for <see cref="VoxelSpacing"/>).
		/// </summary>
		public Rectangle3D VolumeBounds
		{
			get { return _volumeHeader.VolumeBounds; }
		}

		/// <summary>
		/// Gets the spacing between voxels in millimetres (mm).
		/// </summary>
		/// <remarks>
		/// Equivalently, this is the size of a single voxel in millimetres along each dimension, and is the volumetric analogue of an image's Pixel Spacing.
		/// </remarks>
		public Vector3D VoxelSpacing
		{
			get { return _volumeHeader.VoxelSpacing; }
		}

		/// <summary>
		/// Gets the origin of the volume in the patient coordinate system.
		/// </summary>
		/// <remarks>
		/// This is the volumetric analogue of the Image Position (Patient) concept in DICOM.
		/// </remarks>
		public Vector3D VolumePositionPatient
		{
			get { return _volumeHeader.VolumePositionPatient; }
		}

		/// <summary>
		/// Gets the direction of the volume X-axis in the patient coordinate system.
		/// </summary>
		/// <remarks>
		/// This is the volumetric analogue of the Image Orientation (Patient) concept in DICOM.
		/// </remarks>
		public Vector3D VolumeOrientationPatientX
		{
			get { return _volumeHeader.VolumeOrientationPatientX; }
		}

		/// <summary>
		/// Gets the direction of the volume Y-axis in the patient coordinate system.
		/// </summary>
		/// <remarks>
		/// This is the volumetric analogue of the Image Orientation (Patient) concept in DICOM.
		/// </remarks>
		public Vector3D VolumeOrientationPatientY
		{
			get { return _volumeHeader.VolumeOrientationPatientY; }
		}

		/// <summary>
		/// Gets the direction of the volume Z-axis in the patient coordinate system.
		/// </summary>
		/// <remarks>
		/// This is the volumetric analogue of the Image Orientation (Patient) concept in DICOM.
		/// </remarks>
		public Vector3D VolumeOrientationPatientZ
		{
			get { return _volumeHeader.VolumeOrientationPatientZ; }
		}

		/// <summary>
		/// Gets the centre of the volume in the volume coordinate system.
		/// </summary>
		public Vector3D VolumeCenter
		{
			get { return _volumeHeader.VolumeCenter; }
		}

		/// <summary>
		/// Gets the centre of the volume in the patient coordinate system.
		/// </summary>
		public Vector3D VolumeCenterPatient
		{
			get { return _volumeHeader.VolumeCenterPatient; }
		}

		/// <summary>
		/// Gets the value used for padding empty regions of the volume.
		/// </summary>
		public int PaddingValue
		{
			get { return _volumeHeader.PaddingValue; }
		}

		/// <summary>
		/// Gets the minimum voxel value in the volume data.
		/// </summary>
		public int MinimumVolumeValue
		{
			get { return _minVolumeValue.HasValue ? _minVolumeValue.Value : DoGetMinMaxVolumeValue(true); }
		}

		/// <summary>
		/// Gets the maximum voxel value in the volume data.
		/// </summary>
		public int MaximumVolumeValue
		{
			get { return _maxVolumeValue.HasValue ? _maxVolumeValue.Value : DoGetMinMaxVolumeValue(false); }
		}

		/// <summary>
		/// Gets the Modality of the source images from which the volume was created.
		/// </summary>
		public string Modality
		{
			get { return _volumeHeader.Modality; }
		}

		/// <summary>
		/// Gets the Series Instance UID that identifies the source images from which the volume was created.
		/// </summary>
		public string SourceSeriesInstanceUid
		{
			get { return _volumeHeader.SourceSeriesInstanceUid; }
		}

		/// <summary>
		/// Gets the Frame of Reference UID that correlates the patient coordinate system with other data sources.
		/// </summary>
		public string FrameOfReferenceUid
		{
			get { return _volumeHeader.FrameOfReferenceUid; }
		}

		/// <summary>
		/// Gets the DICOM data set containing the common values in the source images from which the volume was created.
		/// </summary>
		/// <remarks>
		/// In general, this data set should be considered read only. This is especially true if you are using the <see cref="Volume"/>
		/// in conjunction with the <see cref="VolumeCache"/>, as any changes are only temporary and will be lost if the volume
		/// instance were unloaded by the memory manager and subsequently reloaded on demand.
		/// </remarks>
		public IVolumeDataSet DataSet
		{
			get { return _volumeHeader; }
		}

		#endregion

		#region Other Protected Methods

		private int DoGetMinMaxVolumeValue(bool returnMin)
		{
			int min, max;
			GetMinMaxVolumeValue(out min, out max);
			_minVolumeValue = min;
			_maxVolumeValue = max;
			return returnMin ? min : max;
		}

		/// <summary>
		/// Called to determine the minimum and maximum voxel values in the volume data.
		/// </summary>
		protected abstract void GetMinMaxVolumeValue(out int minValue, out int maxValue);

		#endregion

		#region Coordinate Transforms

		/// <summary>
		/// Converts the specified volume position into the patient coordinate system.
		/// </summary>
		public Vector3D ConvertToPatient(Vector3D volumePosition)
		{
			return _volumeHeader.ConvertToPatient(volumePosition);
		}

		/// <summary>
		/// Converts the specified patient position into the volume coordinate system.
		/// </summary>
		public Vector3D ConvertToVolume(Vector3D patientPosition)
		{
			return _volumeHeader.ConvertToVolume(patientPosition);
		}

		/// <summary>
		/// Rotates the specified volume orientation matrix into the patient coordinate system.
		/// </summary>
		public Matrix RotateToPatientOrientation(Matrix volumeOrientation)
		{
			return _volumeHeader.RotateToPatientOrientation(volumeOrientation);
		}

		/// <summary>
		/// Rotates the specified patient orientation matrix into the volume coordinate system.
		/// </summary>
		public Matrix RotateToVolumeOrientation(Matrix patientOrientation)
		{
			return _volumeHeader.RotateToVolumeOrientation(patientOrientation);
		}

		/// <summary>
		/// Rotates the specified volume vector into the patient coordinate system.
		/// </summary>
		public Vector3D RotateToPatientOrientation(Vector3D volumeVector)
		{
			return _volumeHeader.RotateToPatientOrientation(volumeVector);
		}

		/// <summary>
		/// Rotates the specified patient vector into the volume coordinate system.
		/// </summary>
		public Vector3D RotateToVolumeOrientation(Vector3D patientVector)
		{
			return _volumeHeader.RotateToVolumeOrientation(patientVector);
		}

		#endregion

		#region Finalizer and Disposal

		~Volume()
		{
			try
			{
				Dispose(false);
			}
			catch (Exception ex)
			{
				Platform.Log(LogLevel.Warn, ex);
			}
		}

		/// <summary>
		/// Called to release any resources held by this object.
		/// </summary>
		/// <param name="disposing">True if the object is being disposed; False if the object is being finalized.</param>
		protected virtual void Dispose(bool disposing) {}

		#endregion
	}
}