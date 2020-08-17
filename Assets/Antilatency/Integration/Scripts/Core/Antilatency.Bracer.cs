//Copyright 2020, ALT LLC. All Rights Reserved.
//This file is part of Antilatency SDK.
//It is subject to the license terms in the LICENSE file found in the top-level directory
//of this distribution and at http://www.antilatency.com/eula
//You may not use this file except in compliance with the License.
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
#pragma warning disable IDE1006 // Do not warn about naming style violations
#pragma warning disable IDE0017 // Do not suggest to simplify object initialization
using System.Runtime.InteropServices; //GuidAttribute
namespace Antilatency.Bracer {

/// <summary>Vibration parameters.</summary>
[System.Serializable]
[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
public partial struct Vibration {
	/// <summary>Vibration intensity value in range 0 ... 1. Value 0 - no vibration, value 1 - max vibration.</summary>
	public float intensity;
	/// <summary>Vibration duration in seconds. Valid range 0.0 ... 65.535</summary>
	public float duration;
}

[Guid("9be46b31-70e4-465b-a60b-9a5e1ed9e820")]
public interface ICotask : Antilatency.DeviceNetwork.ICotaskBatteryPowered {
	/// <summary>Get actual native touch value.</summary>
	/// <returns>Native touch value without any preprocessing. Lower value show stronger touch.</returns>
	uint getTouchNativeValue();
	/// <summary>Get actual touch value. Window for calculation based on values from device properties: "touch/WindowTop" and "touch/WindowBottom". If it doesn't exist, will be used default window(500 .. 760)</summary>
	/// <returns>Touch value after processing in range 0 .. 1. Value 0 - no touch, value 1 - max touch. If value is outside of windows, it will be clamped to range 0 .. 1.</returns>
	float getTouch();
	/// <summary>Play continuous vibration sequence. Note, that You can use 0 intensity for delay in sequence.</summary>
	/// <param name = "sequence">
	/// Array of vibration parameters such intensity and duration. If values in sequence are outside valid range, method will throw invalid argument exception.
	/// </param>
	void executeVibrationSequence(Antilatency.Bracer.Vibration[] sequence);
}
namespace Details {
	public class ICotaskWrapper : Antilatency.DeviceNetwork.Details.ICotaskBatteryPoweredWrapper, ICotask {
		private ICotaskRemap.VMT _VMT = new ICotaskRemap.VMT();
		protected new int GetTotalNativeMethodsCount() {
		    return base.GetTotalNativeMethodsCount() + typeof(ICotaskRemap.VMT).GetFields().Length;
		}
		public ICotaskWrapper(System.IntPtr obj) : base(obj) {
		    _VMT = LoadVMT<ICotaskRemap.VMT>(base.GetTotalNativeMethodsCount());
		}
		public uint getTouchNativeValue() {
			uint result;
			uint resultMarshaler;
			HandleExceptionCode(_VMT.getTouchNativeValue(_object, out resultMarshaler));
			result = resultMarshaler;
			return result;
		}
		public float getTouch() {
			float result;
			float resultMarshaler;
			HandleExceptionCode(_VMT.getTouch(_object, out resultMarshaler));
			result = resultMarshaler;
			return result;
		}
		public void executeVibrationSequence(Antilatency.Bracer.Vibration[] sequence) {
			var sequenceMarshaler = Antilatency.InterfaceContract.Details.ArrayInMarshaler.create(sequence);
			HandleExceptionCode(_VMT.executeVibrationSequence(_object, sequenceMarshaler));
			sequenceMarshaler.Dispose();
		}
	}
	public class ICotaskRemap : Antilatency.DeviceNetwork.Details.ICotaskBatteryPoweredRemap {
		public new struct VMT {
			public delegate Antilatency.InterfaceContract.ExceptionCode getTouchNativeValueDelegate(System.IntPtr _this, out uint result);
			public delegate Antilatency.InterfaceContract.ExceptionCode getTouchDelegate(System.IntPtr _this, out float result);
			public delegate Antilatency.InterfaceContract.ExceptionCode executeVibrationSequenceDelegate(System.IntPtr _this, Antilatency.InterfaceContract.Details.ArrayInMarshaler.Intermediate sequence);
			#pragma warning disable 0649
			public getTouchNativeValueDelegate getTouchNativeValue;
			public getTouchDelegate getTouch;
			public executeVibrationSequenceDelegate executeVibrationSequence;
			#pragma warning restore 0649
		}
		public new static readonly NativeInterfaceVmt NativeVmt;
		static ICotaskRemap() {
			var vmtBlocks = new System.Collections.Generic.List<object>();
			AppendVmt(vmtBlocks);
			NativeVmt = new NativeInterfaceVmt(vmtBlocks);
		}
		protected static new void AppendVmt(System.Collections.Generic.List<object> buffer) {
			Antilatency.DeviceNetwork.Details.ICotaskBatteryPoweredRemap.AppendVmt(buffer);
			var vmt = new VMT();
			vmt.getTouchNativeValue = (System.IntPtr _this, out uint result) => {
				try {
					var obj = GetContext(_this) as ICotask;
					var resultMarshaler = obj.getTouchNativeValue();
					result = resultMarshaler;
				}
				catch (System.Exception ex) {
					result = default(uint);
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			vmt.getTouch = (System.IntPtr _this, out float result) => {
				try {
					var obj = GetContext(_this) as ICotask;
					var resultMarshaler = obj.getTouch();
					result = resultMarshaler;
				}
				catch (System.Exception ex) {
					result = default(float);
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			vmt.executeVibrationSequence = (System.IntPtr _this, Antilatency.InterfaceContract.Details.ArrayInMarshaler.Intermediate sequence) => {
				try {
					var obj = GetContext(_this) as ICotask;
					obj.executeVibrationSequence(sequence.toArray<Antilatency.Bracer.Vibration>());
				}
				catch (System.Exception ex) {
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			buffer.Add(vmt);
		}
		public ICotaskRemap() { }
		public ICotaskRemap(System.IntPtr context, ushort lifetimeId) {
			AllocateNativeInterface(NativeVmt.Handle, context, lifetimeId);
		}
	}
}

[Guid("184969a7-8ba1-4d4b-a3ea-66a5733ce245")]
public interface ICotaskConstructor : Antilatency.DeviceNetwork.ICotaskConstructor {
	Antilatency.Bracer.ICotask startTask(Antilatency.DeviceNetwork.INetwork network, Antilatency.DeviceNetwork.NodeHandle node);
}
namespace Details {
	public class ICotaskConstructorWrapper : Antilatency.DeviceNetwork.Details.ICotaskConstructorWrapper, ICotaskConstructor {
		private ICotaskConstructorRemap.VMT _VMT = new ICotaskConstructorRemap.VMT();
		protected new int GetTotalNativeMethodsCount() {
		    return base.GetTotalNativeMethodsCount() + typeof(ICotaskConstructorRemap.VMT).GetFields().Length;
		}
		public ICotaskConstructorWrapper(System.IntPtr obj) : base(obj) {
		    _VMT = LoadVMT<ICotaskConstructorRemap.VMT>(base.GetTotalNativeMethodsCount());
		}
		public Antilatency.Bracer.ICotask startTask(Antilatency.DeviceNetwork.INetwork network, Antilatency.DeviceNetwork.NodeHandle node) {
			Antilatency.Bracer.ICotask result;
			System.IntPtr resultMarshaler;
			var networkMarshaler = Antilatency.InterfaceContract.Details.InterfaceMarshaler.ManagedToNative<Antilatency.DeviceNetwork.INetwork>(network);
			HandleExceptionCode(_VMT.startTask(_object, networkMarshaler, node, out resultMarshaler));
			result = (resultMarshaler==System.IntPtr.Zero) ? null : new Antilatency.Bracer.Details.ICotaskWrapper(resultMarshaler);
			return result;
		}
	}
	public class ICotaskConstructorRemap : Antilatency.DeviceNetwork.Details.ICotaskConstructorRemap {
		public new struct VMT {
			public delegate Antilatency.InterfaceContract.ExceptionCode startTaskDelegate(System.IntPtr _this, System.IntPtr network, Antilatency.DeviceNetwork.NodeHandle node, out System.IntPtr result);
			#pragma warning disable 0649
			public startTaskDelegate startTask;
			#pragma warning restore 0649
		}
		public new static readonly NativeInterfaceVmt NativeVmt;
		static ICotaskConstructorRemap() {
			var vmtBlocks = new System.Collections.Generic.List<object>();
			AppendVmt(vmtBlocks);
			NativeVmt = new NativeInterfaceVmt(vmtBlocks);
		}
		protected static new void AppendVmt(System.Collections.Generic.List<object> buffer) {
			Antilatency.DeviceNetwork.Details.ICotaskConstructorRemap.AppendVmt(buffer);
			var vmt = new VMT();
			vmt.startTask = (System.IntPtr _this, System.IntPtr network, Antilatency.DeviceNetwork.NodeHandle node, out System.IntPtr result) => {
				try {
					var obj = GetContext(_this) as ICotaskConstructor;
					var networkMarshaler = network == System.IntPtr.Zero ? null : new Antilatency.DeviceNetwork.Details.INetworkWrapper(network);
					var resultMarshaler = obj.startTask(networkMarshaler, node);
					result = Antilatency.InterfaceContract.Details.InterfaceMarshaler.ManagedToNative<Antilatency.Bracer.ICotask>(resultMarshaler);
				}
				catch (System.Exception ex) {
					result = default(System.IntPtr);
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			buffer.Add(vmt);
		}
		public ICotaskConstructorRemap() { }
		public ICotaskConstructorRemap(System.IntPtr context, ushort lifetimeId) {
			AllocateNativeInterface(NativeVmt.Handle, context, lifetimeId);
		}
	}
}

[Guid("24c5255f-88f6-4371-ac9e-67544fe83e09")]
public interface ILibrary : Antilatency.InterfaceContract.IInterface {
	/// <summary>Get version of AntilatencyBracer library.</summary>
	string getVersion();
	/// <summary>Create AntilatencyBracer CotaskConstructor.</summary>
	Antilatency.Bracer.ICotaskConstructor getCotaskConstructor();
}
public static class Library{
    [DllImport("AntilatencyBracer")]
    private static extern Antilatency.InterfaceContract.ExceptionCode getLibraryInterface(System.IntPtr unloader, out System.IntPtr result);
    public static ILibrary load(){
        System.IntPtr libraryAsIInterfaceIntermediate;
        getLibraryInterface(System.IntPtr.Zero, out libraryAsIInterfaceIntermediate);
        Antilatency.InterfaceContract.IInterface libraryAsIInterface = new Antilatency.InterfaceContract.Details.IInterfaceWrapper(libraryAsIInterfaceIntermediate);
        var library = libraryAsIInterface.QueryInterface<ILibrary>();
        libraryAsIInterface.Dispose();
        return library;
    }
}
namespace Details {
	public class ILibraryWrapper : Antilatency.InterfaceContract.Details.IInterfaceWrapper, ILibrary {
		private ILibraryRemap.VMT _VMT = new ILibraryRemap.VMT();
		protected new int GetTotalNativeMethodsCount() {
		    return base.GetTotalNativeMethodsCount() + typeof(ILibraryRemap.VMT).GetFields().Length;
		}
		public ILibraryWrapper(System.IntPtr obj) : base(obj) {
		    _VMT = LoadVMT<ILibraryRemap.VMT>(base.GetTotalNativeMethodsCount());
		}
		public string getVersion() {
			string result;
			var resultMarshaler = Antilatency.InterfaceContract.Details.ArrayOutMarshaler.create();
			HandleExceptionCode(_VMT.getVersion(_object, resultMarshaler));
			result = resultMarshaler.value;
			resultMarshaler.Dispose();
			return result;
		}
		public Antilatency.Bracer.ICotaskConstructor getCotaskConstructor() {
			Antilatency.Bracer.ICotaskConstructor result;
			System.IntPtr resultMarshaler;
			HandleExceptionCode(_VMT.getCotaskConstructor(_object, out resultMarshaler));
			result = (resultMarshaler==System.IntPtr.Zero) ? null : new Antilatency.Bracer.Details.ICotaskConstructorWrapper(resultMarshaler);
			return result;
		}
	}
	public class ILibraryRemap : Antilatency.InterfaceContract.Details.IInterfaceRemap {
		public new struct VMT {
			public delegate Antilatency.InterfaceContract.ExceptionCode getVersionDelegate(System.IntPtr _this, Antilatency.InterfaceContract.Details.ArrayOutMarshaler.Intermediate result);
			public delegate Antilatency.InterfaceContract.ExceptionCode getCotaskConstructorDelegate(System.IntPtr _this, out System.IntPtr result);
			#pragma warning disable 0649
			public getVersionDelegate getVersion;
			public getCotaskConstructorDelegate getCotaskConstructor;
			#pragma warning restore 0649
		}
		public new static readonly NativeInterfaceVmt NativeVmt;
		static ILibraryRemap() {
			var vmtBlocks = new System.Collections.Generic.List<object>();
			AppendVmt(vmtBlocks);
			NativeVmt = new NativeInterfaceVmt(vmtBlocks);
		}
		protected static new void AppendVmt(System.Collections.Generic.List<object> buffer) {
			Antilatency.InterfaceContract.Details.IInterfaceRemap.AppendVmt(buffer);
			var vmt = new VMT();
			vmt.getVersion = (System.IntPtr _this, Antilatency.InterfaceContract.Details.ArrayOutMarshaler.Intermediate result) => {
				try {
					var obj = GetContext(_this) as ILibrary;
					var resultMarshaler = obj.getVersion();
					result.assign(resultMarshaler);
				}
				catch (System.Exception ex) {
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			vmt.getCotaskConstructor = (System.IntPtr _this, out System.IntPtr result) => {
				try {
					var obj = GetContext(_this) as ILibrary;
					var resultMarshaler = obj.getCotaskConstructor();
					result = Antilatency.InterfaceContract.Details.InterfaceMarshaler.ManagedToNative<Antilatency.Bracer.ICotaskConstructor>(resultMarshaler);
				}
				catch (System.Exception ex) {
					result = default(System.IntPtr);
					return handleRemapException(ex, _this);
				}
				return Antilatency.InterfaceContract.ExceptionCode.Ok;
			};
			buffer.Add(vmt);
		}
		public ILibraryRemap() { }
		public ILibraryRemap(System.IntPtr context, ushort lifetimeId) {
			AllocateNativeInterface(NativeVmt.Handle, context, lifetimeId);
		}
	}
}

public static partial class Constants {
	public const string TouchWindowTopName = "touch/WindowTop";
	public const string TouchWindowBottomName = "touch/WindowBottom";
}


}
