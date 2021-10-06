using RosMessageTypes.Geometry;
using System;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector.ROSGeometry
{
    public interface ICoordinateSpace
    {
        Vector3 ConvertFromRUF(Vector3 v); // convert this vector from the Unity coordinate space into mine
        Vector3 ConvertToRUF(Vector3 v); // convert from my coordinate space into the Unity coordinate space

        Quaternion ConvertFromRUF(Quaternion q); // convert this quaternion from the Unity coordinate space into mine
        Quaternion ConvertToRUF(Quaternion q); // convert from my coordinate space into the Unity coordinate space
    }

    [Obsolete("CoordinateSpace has been renamed to ICoordinateSpace")]
    public interface CoordinateSpace : ICoordinateSpace
    {
    }

    //RUF is the Unity coordinate space, so no conversion needed
    public class RUF : ICoordinateSpace
    {
        public Vector3 ConvertFromRUF(Vector3 v) => v;
        public Vector3 ConvertToRUF(Vector3 v) => v;
        public Quaternion ConvertFromRUF(Quaternion q) => q;
        public Quaternion ConvertToRUF(Quaternion q) => q;
    }

    public class FLU : ICoordinateSpace
    {
        public Vector3 ConvertFromRUF(Vector3 v) => new Vector3(v.z, -v.x, v.y);
        public Vector3 ConvertToRUF(Vector3 v) => new Vector3(-v.y, v.z, v.x);
        public Quaternion ConvertFromRUF(Quaternion q) => new Quaternion(q.z, -q.x, q.y, -q.w);
        public Quaternion ConvertToRUF(Quaternion q) => new Quaternion(-q.y, q.z, q.x, -q.w);
    }

    public class ENULocal : FLU { }

    public class FRD : ICoordinateSpace
    {
        public Vector3 ConvertFromRUF(Vector3 v) => new Vector3(v.z, v.x, -v.y);
        public Vector3 ConvertToRUF(Vector3 v) => new Vector3(v.y, -v.z, v.x);
        public Quaternion ConvertFromRUF(Quaternion q) => new Quaternion(q.z, q.x, -q.y, -q.w);
        public Quaternion ConvertToRUF(Quaternion q) => new Quaternion(q.y, -q.z, q.x, -q.w);
    }

    public class NEDLocal : FRD { }

    public class NED : ICoordinateSpace
    {
        public Vector3 ConvertFromRUF(Vector3 v)
        {
            var vNed = GeometryCompass.ToNED(v);
            return new Vector3(vNed.x, vNed.y, vNed.z);
        }

        public Vector3 ConvertToRUF(Vector3 v) => GeometryCompass.FromNED(new Vector3<NED>(v.x, v.y, v.z));

        public Quaternion ConvertFromRUF(Quaternion q)
        {
            var qNed = GeometryCompass.ToNED(q);
            return new Quaternion(qNed.x, qNed.y, qNed.z, qNed.w);
        }

        public Quaternion ConvertToRUF(Quaternion q) => GeometryCompass.FromNED(new Quaternion<NED>(q.x, q.y, q.z, q.w));
    }

    public class ENU : ICoordinateSpace
    {
        public Vector3 ConvertFromRUF(Vector3 v)
        {
            var vEnu = GeometryCompass.ToENU(v);
            return new Vector3(vEnu.x, vEnu.y, vEnu.z);
        }
        public Vector3 ConvertToRUF(Vector3 v) => GeometryCompass.FromENU(new Vector3<ENU>(v.x, v.y, v.z));

        public Quaternion ConvertFromRUF(Quaternion q)
        {
            var qEnu = GeometryCompass.ToENU(q);
            return new Quaternion(qEnu.x, qEnu.y, qEnu.z, qEnu.w);
        }

        public Quaternion ConvertToRUF(Quaternion q) => GeometryCompass.FromENU(new Quaternion<ENU>(q.x, q.y, q.z, q.w));
    }

    public enum CoordinateSpaceSelection
    {
        RUF,
        FLU,
        FRD,
        NED,
        ENU,
        NEDLocal,
        ENULocal,
    }

    public static class CoordinateSpaceExtensions
    {
        public static Vector3<C> To<C>(this Vector3 self)
            where C : ICoordinateSpace, new()
        {
            return new Vector3<C>(self);
        }

        public static Quaternion<C> To<C>(this Quaternion self)
            where C : ICoordinateSpace, new()
        {
            return new Quaternion<C>(self);
        }

        public static Vector3<C> As<C>(this PointMsg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>((float)self.x, (float)self.y, (float)self.z);
        }

        public static Vector3 From<C>(this PointMsg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>((float)self.x, (float)self.y, (float)self.z).toUnity;
        }

        public static Vector3<C> As<C>(this Point32Msg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>(self.x, self.y, self.z);
        }

        public static Vector3 From<C>(this Point32Msg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>(self.x, self.y, self.z).toUnity;
        }

        public static Vector3<C> As<C>(this Vector3Msg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>((float)self.x, (float)self.y, (float)self.z);
        }

        public static Vector3 From<C>(this Vector3Msg self) where C : ICoordinateSpace, new()
        {
            return new Vector3<C>((float)self.x, (float)self.y, (float)self.z).toUnity;
        }

        public static Quaternion<C> As<C>(this QuaternionMsg self) where C : ICoordinateSpace, new()
        {
            return new Quaternion<C>((float)self.x, (float)self.y, (float)self.z, (float)self.w);
        }

        public static Quaternion From<C>(this QuaternionMsg self) where C : ICoordinateSpace, new()
        {
            return new Quaternion<C>((float)self.x, (float)self.y, (float)self.z, (float)self.w).toUnity;
        }

        public static TransformMsg To<C>(this Transform transform) where C : ICoordinateSpace, new()
        {
            return new TransformMsg(new Vector3<C>(transform.position), new Quaternion<C>(transform.rotation));
        }

        public static TransformMsg ToLocal<C>(this Transform transform) where C : ICoordinateSpace, new()
        {
            return new TransformMsg(new Vector3<C>(transform.localPosition), new Quaternion<C>(transform.localRotation));
        }

        public static Vector3 From(this PointMsg self, CoordinateSpaceSelection selection)
        {
            switch (selection)
            {
                case CoordinateSpaceSelection.RUF:
                    return self.From<RUF>();
                case CoordinateSpaceSelection.FLU:
                    return self.From<FLU>();
                case CoordinateSpaceSelection.FRD:
                    return self.From<FRD>();
                case CoordinateSpaceSelection.ENU:
                    return self.From<ENU>();
                case CoordinateSpaceSelection.NED:
                    return self.From<NED>();
                case CoordinateSpaceSelection.ENULocal:
                    return self.From<ENULocal>();
                case CoordinateSpaceSelection.NEDLocal:
                    return self.From<NEDLocal>();
                default:
                    Debug.LogError("Invalid coordinate space " + selection);
                    return self.From<RUF>();
            }
        }

        public static Vector3 From(this Point32Msg self, CoordinateSpaceSelection selection)
        {
            switch (selection)
            {
                case CoordinateSpaceSelection.RUF:
                    return self.From<RUF>();
                case CoordinateSpaceSelection.FLU:
                    return self.From<FLU>();
                case CoordinateSpaceSelection.FRD:
                    return self.From<FRD>();
                case CoordinateSpaceSelection.ENU:
                    return self.From<ENU>();
                case CoordinateSpaceSelection.NED:
                    return self.From<NED>();
                case CoordinateSpaceSelection.ENULocal:
                    return self.From<ENULocal>();
                case CoordinateSpaceSelection.NEDLocal:
                    return self.From<NEDLocal>();
                default:
                    Debug.LogError("Invalid coordinate space " + selection);
                    return self.From<RUF>();
            }
        }

        public static Vector3 From(this Vector3Msg self, CoordinateSpaceSelection selection)
        {
            switch (selection)
            {
                case CoordinateSpaceSelection.RUF:
                    return self.From<RUF>();
                case CoordinateSpaceSelection.FLU:
                    return self.From<FLU>();
                case CoordinateSpaceSelection.FRD:
                    return self.From<FRD>();
                case CoordinateSpaceSelection.ENU:
                    return self.From<ENU>();
                case CoordinateSpaceSelection.NED:
                    return self.From<NED>();
                case CoordinateSpaceSelection.ENULocal:
                    return self.From<ENULocal>();
                case CoordinateSpaceSelection.NEDLocal:
                    return self.From<NEDLocal>();
                default:
                    Debug.LogError("Invalid coordinate space " + selection);
                    return self.From<RUF>();
            }
        }

        public static Quaternion From(this QuaternionMsg self, CoordinateSpaceSelection selection)
        {
            switch (selection)
            {
                case CoordinateSpaceSelection.RUF:
                    return self.From<RUF>();
                case CoordinateSpaceSelection.FLU:
                    return self.From<FLU>();
                case CoordinateSpaceSelection.FRD:
                    return self.From<FRD>();
                case CoordinateSpaceSelection.ENU:
                    return self.From<ENU>();
                case CoordinateSpaceSelection.NED:
                    return self.From<NED>();
                case CoordinateSpaceSelection.ENULocal:
                    return self.From<ENULocal>();
                case CoordinateSpaceSelection.NEDLocal:
                    return self.From<NEDLocal>();
                default:
                    Debug.LogError("Invalid coordinate space " + selection);
                    return self.From<RUF>();
            }
        }
    }
}
