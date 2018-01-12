using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SIMDArray
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var size = 1000;
            var arr = new float[size];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = i;

            var arr1 = new float[size];
            for (var i = 0; i < arr.Length; i++)
                arr1[i] = i * 2;

            var s0 = new VectorSimd(arr);
            var s1 = new VectorSimd(arr1);
            var v0 = new MyVector(arr);
            var v1 = new MyVector(arr1);

            var sw = new Stopwatch();
            sw.Start();


            for (var i = 0; i < 100000; i++)
            {
                var tmp = VectorSimd.Add(s0, s1);
            }


            sw.Stop();
            long a = sw.ElapsedMilliseconds;
            Console.WriteLine("vs=" + a);
            sw.Reset();
            sw.Start();


            for (var i = 0; i < 100000; i++)
            {
                var tmp = MyVector.Add(v0, v1);
            }
            sw.Stop();
            long b = sw.ElapsedMilliseconds;

            Console.WriteLine("mv=" + b);
            Console.WriteLine(1.0*b/a);
        }
    }

    internal class MyVector
    {
        private readonly float[] arr;

        public MyVector(int size)
        {
            arr = new float[size];
        }

        public MyVector(float[] f)
        {
            arr = f;
        }

        public static MyVector Add(MyVector v0, float v)
        {
            var rtn = new MyVector(v0.arr.Length);
            for (var i = 0; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v;
            return rtn;
        }

        public static MyVector Add(MyVector v0, MyVector v1)
        {
            var rtn = new MyVector(v0.arr.Length);
            for (var i = 0; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v1.arr[i];
            return rtn;
        }
    }

    internal unsafe class VectorSimd : IEnumerable<float>

    {
        public float[] arr;
        private GCHandle g;

        public VectorSimd(int Size)
        {
            arr = new float[Size];
        }

        public float this[int n]
        {
            get => arr[n];
            set => arr[n] = value;
        }


        public VectorSimd(float[] v)
        {
            arr = v;
        }

        private float* Alloc()
        {
            g = GCHandle.Alloc(arr);
            return (float*) g.AddrOfPinnedObject();
        }

        private void Free()
        {
            g.Free();
        }

        private static int GetArrayCount(VectorSimd v)
        {
            return v.arr.Length % 4 == 0 ? v.arr.Length / 4 : v.arr.Length / 4 + 1;
        }

        private static int GetLoopCount(VectorSimd v)
        {
            return v.arr.Length / 4;
        }
        

        #region Subtract

        public static VectorSimd operator -(VectorSimd v0, VectorSimd v1)
        {
            return Subtract(v0, v1);
        }

        public static VectorSimd operator -(VectorSimd v0, float v)
        {
            return Subtract(v0, v);
        }

        public static VectorSimd operator -(float v, VectorSimd v0)
        {
            return Subtract(v, v0);
        }

        public static VectorSimd Subtract(float v, VectorSimd v0)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);
            var v1 = new Vector4(v, v, v, v);
            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = &v1;
                var p1 = (Vector4*)vec0;
                var r = (Vector4*)rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Subtract(*p0, *p1);
                    r++;
                    p1++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v;
            return rtn;
        }

        public static VectorSimd Subtract(VectorSimd v0, float v)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);
            var v1 = new Vector4(v, v, v, v);
            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = (Vector4*)vec0;
                var p1 = &v1;
                var r = (Vector4*)rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Subtract(*p0, *p1);
                    r++;
                    p0++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v;
            return rtn;
        }

        public static VectorSimd Subtract(VectorSimd v0, VectorSimd v1)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);

            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* vec1 = &v1.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = (Vector4*)vec0;
                var p1 = (Vector4*)vec1;
                var r = (Vector4*)rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Subtract(*p0, *p1);
                    r++;
                    p0++;
                    p1++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v1.arr[i];
            return rtn;
        }

        #endregion


        #region Add

        public static VectorSimd operator +(VectorSimd v0, VectorSimd v1)
        {
            return Add(v0, v1);
        }

        public static VectorSimd operator +(VectorSimd v0, float v)
        {
            return Add(v0, v);
        }

        public static VectorSimd operator +(float v, VectorSimd v0)
        {
            return Add(v, v0);
        }

        public static VectorSimd Add(float v, VectorSimd v0)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);
            var v1 = new Vector4(v, v, v, v);
            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = (Vector4*) vec0;
                var p1 = &v1;
                var r = (Vector4*) rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Add(*p0, *p1);
                    r++;
                    p0++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v;
            return rtn;
        }

        public static VectorSimd Add(VectorSimd v0, float v)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);
            var v1 = new Vector4(v, v, v, v);
            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = (Vector4*) vec0;
                var p1 = &v1;
                var r = (Vector4*) rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Add(*p0, *p1);
                    r++;
                    p0++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v;
            return rtn;
        }

        public static VectorSimd Add(VectorSimd v0, VectorSimd v1)
        {
            var rtn = new VectorSimd(v0.arr.Length);
            var loopNum = GetLoopCount(v0);

            //  vec0;
            fixed (float* vec0 = &v0.arr[0])
            fixed (float* vec1 = &v1.arr[0])
            fixed (float* rtnp = &rtn.arr[0])
            {
                var p0 = (Vector4*) vec0;
                var p1 = (Vector4*) vec1;
                var r = (Vector4*) rtnp;
                for (var i = 0; i < loopNum; i++)
                {
                    *r = Vector4.Add(*p0, *p1);
                    r++;
                    p0++;
                    p1++;
                }
            }
            for (var i = loopNum * 4; i < v0.arr.Length; i++)
                rtn.arr[i] = v0.arr[i] + v1.arr[i];
            return rtn;
        }

     

        #endregion

        #region linq

        public IEnumerator<float> GetEnumerator()
        {
            foreach (var n in arr)
                yield return n;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}