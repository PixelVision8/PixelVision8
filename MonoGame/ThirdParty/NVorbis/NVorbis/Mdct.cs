/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace NVorbis
{
    class Mdct
    {
        const float M_PI = 3.14159265358979323846264f;

        static Dictionary<int, Mdct> _setupCache = new Dictionary<int, Mdct>(2);

        public static void Reverse(float[] samples, int sampleCount)
        {
            GetSetup(sampleCount).CalcReverse(samples);
        }

        static Mdct GetSetup(int n)
        {
            lock (_setupCache)
            {
                if (!_setupCache.ContainsKey(n))
                {
                    _setupCache[n] = new Mdct(n);
                }

                return _setupCache[n];
            }
        }

        int _n, _n2, _n4, _n8, _ld;

        float[] _A, _B, _C;
        ushort[] _bitrev;

        private Mdct(int n)
        {
            this._n = n;
            _n2 = n >> 1;
            _n4 = _n2 >> 1;
            _n8 = _n4 >> 1;

            _ld = Utils.ilog(n) - 1;

            // first, calc the "twiddle factors"
            _A = new float[_n2];
            _B = new float[_n2];
            _C = new float[_n4];
            int k, k2;
            for (k = k2 = 0; k < _n4; ++k, k2 += 2)
            {
                _A[k2] = (float)Math.Cos(4 * k * M_PI / n);
                _A[k2 + 1] = (float)-Math.Sin(4 * k * M_PI / n);
                _B[k2] = (float)Math.Cos((k2 + 1) * M_PI / n / 2) * .5f;
                _B[k2 + 1] = (float)Math.Sin((k2 + 1) * M_PI / n / 2) * .5f;
            }
            for (k = k2 = 0; k < _n8; ++k, k2 += 2)
            {
                _C[k2] = (float)Math.Cos(2 * (k2 + 1) * M_PI / n);
                _C[k2 + 1] = (float)-Math.Sin(2 * (k2 + 1) * M_PI / n);
            }

            // now, calc the bit reverse table
            _bitrev = new ushort[_n8];
            for (int i = 0; i < _n8; ++i)
            {
                _bitrev[i] = (ushort)(Utils.BitReverse((uint)i, _ld - 3) << 2);
            }
        }

        #region Buffer Handling

        // This addresses the two constraints we have to deal with:
        //  1) Each Mdct instance must maintain a buffer of n / 2 size without allocating each pass
        //  2) Mdct must be thread-safe
        // To handle these constraints, we use a "thread-local" dictionary

        Dictionary<int, float[]> _threadLocalBuffers = new Dictionary<int, float[]>(1);
        float[] GetBuffer()
        {
            lock (_threadLocalBuffers)
            {
                float[] buf;
                if (!_threadLocalBuffers.TryGetValue(System.Threading.Thread.CurrentThread.ManagedThreadId, out buf))
                {
                    _threadLocalBuffers[System.Threading.Thread.CurrentThread.ManagedThreadId] = (buf = new float[_n2]);
                }
                return buf;
            }
        }

        #endregion

        void CalcReverse(float[] buffer)
        {
            float[] u, v, buf2;

            buf2 = GetBuffer();

            // copy and reflect spectral data
            // step 0

            {
                var d = _n2 - 2; // buf2
                var AA = 0;     // A
                var e = 0;      // buffer
                var e_stop = _n2;// buffer
                while (e != e_stop)
                {
                    buf2[d + 1] = (buffer[e] * _A[AA] - buffer[e + 2] * _A[AA + 1]);
                    buf2[d] = (buffer[e] * _A[AA + 1] + buffer[e + 2] * _A[AA]);
                    d -= 2;
                    AA += 2;
                    e += 4;
                }

                e = _n2 - 3;
                while (d >= 0)
                {
                    buf2[d + 1] = (-buffer[e + 2] * _A[AA] - -buffer[e] * _A[AA + 1]);
                    buf2[d] = (-buffer[e + 2] * _A[AA + 1] + -buffer[e] * _A[AA]);
                    d -= 2;
                    AA += 2;
                    e -= 4;
                }
            }

            // apply "symbolic" names
            u = buffer;
            v = buf2;

            // step 2

            {
                var AA = _n2 - 8;    // A

                var e0 = _n4;        // v
                var e1 = 0;         // v

                var d0 = _n4;        // u
                var d1 = 0;         // u

                while (AA >= 0)
                {
                    float v40_20, v41_21;

                    v41_21 = v[e0 + 1] - v[e1 + 1];
                    v40_20 = v[e0] - v[e1];
                    u[d0 + 1] = v[e0 + 1] + v[e1 + 1];
                    u[d0] = v[e0] + v[e1];
                    u[d1 + 1] = v41_21 * _A[AA + 4] - v40_20 * _A[AA + 5];
                    u[d1] = v40_20 * _A[AA + 4] + v41_21 * _A[AA + 5];

                    v41_21 = v[e0 + 3] - v[e1 + 3];
                    v40_20 = v[e0 + 2] - v[e1 + 2];
                    u[d0 + 3] = v[e0 + 3] + v[e1 + 3];
                    u[d0 + 2] = v[e0 + 2] + v[e1 + 2];
                    u[d1 + 3] = v41_21 * _A[AA] - v40_20 * _A[AA + 1];
                    u[d1 + 2] = v40_20 * _A[AA] + v41_21 * _A[AA + 1];

                    AA -= 8;

                    d0 += 4;
                    d1 += 4;
                    e0 += 4;
                    e1 += 4;
                }
            }

            // step 3

            // iteration 0
            step3_iter0_loop(_n >> 4, u, _n2 - 1 - _n4 * 0, -_n8);
            step3_iter0_loop(_n >> 4, u, _n2 - 1 - _n4 * 1, -_n8);

            // iteration 1
            step3_inner_r_loop(_n >> 5, u, _n2 - 1 - _n8 * 0, -(_n >> 4), 16);
            step3_inner_r_loop(_n >> 5, u, _n2 - 1 - _n8 * 1, -(_n >> 4), 16);
            step3_inner_r_loop(_n >> 5, u, _n2 - 1 - _n8 * 2, -(_n >> 4), 16);
            step3_inner_r_loop(_n >> 5, u, _n2 - 1 - _n8 * 3, -(_n >> 4), 16);

            // iterations 2 ... x
            var l = 2;
            for (; l < (_ld - 3) >> 1; ++l)
            {
                var k0 = _n >> (l + 2);
                var k0_2 = k0 >> 1;
                var lim = 1 << (l + 1);
                for (int i = 0; i < lim; ++i)
                {
                    step3_inner_r_loop(_n >> (l + 4), u, _n2 - 1 - k0 * i, -k0_2, 1 << (l + 3));
                }
            }

            // iterations x ... end
            for (; l < _ld - 6; ++l)
            {
                var k0 = _n >> (l + 2);
                var k1 = 1 << (l + 3);
                var k0_2 = k0 >> 1;
                var rlim = _n >> (l + 6);
                var lim = 1 << l + 1;
                var i_off = _n2 - 1;
                var A0 = 0;

                for (int r = rlim; r > 0; --r)
                {
                    step3_inner_s_loop(lim, u, i_off, -k0_2, A0, k1, k0);
                    A0 += k1 * 4;
                    i_off -= 8;
                }
            }

            // combine some iteration steps...
            step3_inner_s_loop_ld654(_n >> 5, u, _n2 - 1, _n);

            // steps 4, 5, and 6
            {
                var bit = 0;

                var d0 = _n4 - 4;    // v
                var d1 = _n2 - 4;    // v
                while (d0 >= 0)
                {
                    int k4;

                    k4 = _bitrev[bit];
                    v[d1 + 3] = u[k4];
                    v[d1 + 2] = u[k4 + 1];
                    v[d0 + 3] = u[k4 + 2];
                    v[d0 + 2] = u[k4 + 3];

                    k4 = _bitrev[bit + 1];
                    v[d1 + 1] = u[k4];
                    v[d1] = u[k4 + 1];
                    v[d0 + 1] = u[k4 + 2];
                    v[d0] = u[k4 + 3];

                    d0 -= 4;
                    d1 -= 4;
                    bit += 2;
                }
            }

            // step 7
            {
                var c = 0;      // C
                var d = 0;      // v
                var e = _n2 - 4; // v

                while (d < e)
                {
                    float a02, a11, b0, b1, b2, b3;

                    a02 = v[d] - v[e + 2];
                    a11 = v[d + 1] + v[e + 3];

                    b0 = _C[c + 1] * a02 + _C[c] * a11;
                    b1 = _C[c + 1] * a11 - _C[c] * a02;

                    b2 = v[d] + v[e + 2];
                    b3 = v[d + 1] - v[e + 3];

                    v[d] = b2 + b0;
                    v[d + 1] = b3 + b1;
                    v[e + 2] = b2 - b0;
                    v[e + 3] = b1 - b3;

                    a02 = v[d + 2] - v[e];
                    a11 = v[d + 3] + v[e + 1];

                    b0 = _C[c + 3] * a02 + _C[c + 2] * a11;
                    b1 = _C[c + 3] * a11 - _C[c + 2] * a02;

                    b2 = v[d + 2] + v[e];
                    b3 = v[d + 3] - v[e + 1];

                    v[d + 2] = b2 + b0;
                    v[d + 3] = b3 + b1;
                    v[e] = b2 - b0;
                    v[e + 1] = b1 - b3;

                    c += 4;
                    d += 4;
                    e -= 4;
                }
            }

            // step 8 + decode
            {
                var b = _n2 - 8; // B
                var e = _n2 - 8; // buf2
                var d0 = 0;     // buffer
                var d1 = _n2 - 4;// buffer
                var d2 = _n2;    // buffer
                var d3 = _n - 4; // buffer
                while (e >= 0)
                {
                    float p0, p1, p2, p3;

                    p3 = buf2[e + 6] * _B[b + 7] - buf2[e + 7] * _B[b + 6];
                    p2 = -buf2[e + 6] * _B[b + 6] - buf2[e + 7] * _B[b + 7];

                    buffer[d0] = p3;
                    buffer[d1 + 3] = -p3;
                    buffer[d2] = p2;
                    buffer[d3 + 3] = p2;

                    p1 = buf2[e + 4] * _B[b + 5] - buf2[e + 5] * _B[b + 4];
                    p0 = -buf2[e + 4] * _B[b + 4] - buf2[e + 5] * _B[b + 5];

                    buffer[d0 + 1] = p1;
                    buffer[d1 + 2] = -p1;
                    buffer[d2 + 1] = p0;
                    buffer[d3 + 2] = p0;


                    p3 = buf2[e + 2] * _B[b + 3] - buf2[e + 3] * _B[b + 2];
                    p2 = -buf2[e + 2] * _B[b + 2] - buf2[e + 3] * _B[b + 3];

                    buffer[d0 + 2] = p3;
                    buffer[d1 + 1] = -p3;
                    buffer[d2 + 2] = p2;
                    buffer[d3 + 1] = p2;

                    p1 = buf2[e] * _B[b + 1] - buf2[e + 1] * _B[b];
                    p0 = -buf2[e] * _B[b] - buf2[e + 1] * _B[b + 1];

                    buffer[d0 + 3] = p1;
                    buffer[d1] = -p1;
                    buffer[d2 + 3] = p0;
                    buffer[d3] = p0;

                    b -= 8;
                    e -= 8;
                    d0 += 4;
                    d2 += 4;
                    d1 -= 4;
                    d3 -= 4;
                }
            }
        }

        void step3_iter0_loop(int n, float[] e, int i_off, int k_off)
        {
            var ee0 = i_off;        // e
            var ee2 = ee0 + k_off;  // e
            var a = 0;
            for (int i = n >> 2; i > 0; --i)
            {
                float k00_20, k01_21;

                k00_20 = e[ee0] - e[ee2];
                k01_21 = e[ee0 - 1] - e[ee2 - 1];
                e[ee0] += e[ee2];
                e[ee0 - 1] += e[ee2 - 1];
                e[ee2] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[ee2 - 1] = k01_21 * _A[a] + k00_20 * _A[a + 1];
                a += 8;

                k00_20 = e[ee0 - 2] - e[ee2 - 2];
                k01_21 = e[ee0 - 3] - e[ee2 - 3];
                e[ee0 - 2] += e[ee2 - 2];
                e[ee0 - 3] += e[ee2 - 3];
                e[ee2 - 2] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[ee2 - 3] = k01_21 * _A[a] + k00_20 * _A[a + 1];
                a += 8;

                k00_20 = e[ee0 - 4] - e[ee2 - 4];
                k01_21 = e[ee0 - 5] - e[ee2 - 5];
                e[ee0 - 4] += e[ee2 - 4];
                e[ee0 - 5] += e[ee2 - 5];
                e[ee2 - 4] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[ee2 - 5] = k01_21 * _A[a] + k00_20 * _A[a + 1];
                a += 8;

                k00_20 = e[ee0 - 6] - e[ee2 - 6];
                k01_21 = e[ee0 - 7] - e[ee2 - 7];
                e[ee0 - 6] += e[ee2 - 6];
                e[ee0 - 7] += e[ee2 - 7];
                e[ee2 - 6] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[ee2 - 7] = k01_21 * _A[a] + k00_20 * _A[a + 1];
                a += 8;

                ee0 -= 8;
                ee2 -= 8;
            }
        }

        void step3_inner_r_loop(int lim, float[] e, int d0, int k_off, int k1)
        {
            float k00_20, k01_21;

            var e0 = d0;            // e
            var e2 = e0 + k_off;    // e
            int a = 0;

            for (int i = lim >> 2; i > 0; --i)
            {
                k00_20 = e[e0] - e[e2];
                k01_21 = e[e0 - 1] - e[e2 - 1];
                e[e0] += e[e2];
                e[e0 - 1] += e[e2 - 1];
                e[e2] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[e2 - 1] = k01_21 * _A[a] + k00_20 * _A[a + 1];

                a += k1;

                k00_20 = e[e0 - 2] - e[e2 - 2];
                k01_21 = e[e0 - 3] - e[e2 - 3];
                e[e0 - 2] += e[e2 - 2];
                e[e0 - 3] += e[e2 - 3];
                e[e2 - 2] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[e2 - 3] = k01_21 * _A[a] + k00_20 * _A[a + 1];

                a += k1;

                k00_20 = e[e0 - 4] - e[e2 - 4];
                k01_21 = e[e0 - 5] - e[e2 - 5];
                e[e0 - 4] += e[e2 - 4];
                e[e0 - 5] += e[e2 - 5];
                e[e2 - 4] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[e2 - 5] = k01_21 * _A[a] + k00_20 * _A[a + 1];

                a += k1;

                k00_20 = e[e0 - 6] - e[e2 - 6];
                k01_21 = e[e0 - 7] - e[e2 - 7];
                e[e0 - 6] += e[e2 - 6];
                e[e0 - 7] += e[e2 - 7];
                e[e2 - 6] = k00_20 * _A[a] - k01_21 * _A[a + 1];
                e[e2 - 7] = k01_21 * _A[a] + k00_20 * _A[a + 1];

                a += k1;

                e0 -= 8;
                e2 -= 8;
            }
        }

        void step3_inner_s_loop(int n, float[] e, int i_off, int k_off, int a, int a_off, int k0)
        {
            var A0 = _A[a];
            var A1 = _A[a + 1];
            var A2 = _A[a + a_off];
            var A3 = _A[a + a_off + 1];
            var A4 = _A[a + a_off * 2];
            var A5 = _A[a + a_off * 2 + 1];
            var A6 = _A[a + a_off * 3];
            var A7 = _A[a + a_off * 3 + 1];

            float k00, k11;

            var ee0 = i_off;        // e
            var ee2 = ee0 + k_off;  // e

            for (int i = n; i > 0; --i)
            {
                k00 = e[ee0] - e[ee2];
                k11 = e[ee0 - 1] - e[ee2 - 1];
                e[ee0] += e[ee2];
                e[ee0 - 1] += e[ee2 - 1];
                e[ee2] = k00 * A0 - k11 * A1;
                e[ee2 - 1] = k11 * A0 + k00 * A1;

                k00 = e[ee0 - 2] - e[ee2 - 2];
                k11 = e[ee0 - 3] - e[ee2 - 3];
                e[ee0 - 2] += e[ee2 - 2];
                e[ee0 - 3] += e[ee2 - 3];
                e[ee2 - 2] = k00 * A2 - k11 * A3;
                e[ee2 - 3] = k11 * A2 + k00 * A3;

                k00 = e[ee0 - 4] - e[ee2 - 4];
                k11 = e[ee0 - 5] - e[ee2 - 5];
                e[ee0 - 4] += e[ee2 - 4];
                e[ee0 - 5] += e[ee2 - 5];
                e[ee2 - 4] = k00 * A4 - k11 * A5;
                e[ee2 - 5] = k11 * A4 + k00 * A5;

                k00 = e[ee0 - 6] - e[ee2 - 6];
                k11 = e[ee0 - 7] - e[ee2 - 7];
                e[ee0 - 6] += e[ee2 - 6];
                e[ee0 - 7] += e[ee2 - 7];
                e[ee2 - 6] = k00 * A6 - k11 * A7;
                e[ee2 - 7] = k11 * A6 + k00 * A7;

                ee0 -= k0;
                ee2 -= k0;
            }
        }

        void step3_inner_s_loop_ld654(int n, float[] e, int i_off, int base_n)
        {
            var a_off = base_n >> 3;
            var A2 = _A[a_off];
            var z = i_off;          // e
            var @base = z - 16 * n; // e

            while (z > @base)
            {
                float k00, k11;

                k00 = e[z] - e[z - 8];
                k11 = e[z - 1] - e[z - 9];
                e[z] += e[z - 8];
                e[z - 1] += e[z - 9];
                e[z - 8] = k00;
                e[z - 9] = k11;

                k00 = e[z - 2] - e[z - 10];
                k11 = e[z - 3] - e[z - 11];
                e[z - 2] += e[z - 10];
                e[z - 3] += e[z - 11];
                e[z - 10] = (k00 + k11) * A2;
                e[z - 11] = (k11 - k00) * A2;

                k00 = e[z - 12] - e[z - 4];
                k11 = e[z - 5] - e[z - 13];
                e[z - 4] += e[z - 12];
                e[z - 5] += e[z - 13];
                e[z - 12] = k11;
                e[z - 13] = k00;

                k00 = e[z - 14] - e[z - 6];
                k11 = e[z - 7] - e[z - 15];
                e[z - 6] += e[z - 14];
                e[z - 7] += e[z - 15];
                e[z - 14] = (k00 + k11) * A2;
                e[z - 15] = (k00 - k11) * A2;

                iter_54(e, z);
                iter_54(e, z - 8);

                z -= 16;
            }
        }

        private void iter_54(float[] e, int z)
        {
            float k00, k11, k22, k33;
            float y0, y1, y2, y3;

            k00 = e[z] - e[z - 4];
            y0 = e[z] + e[z - 4];
            y2 = e[z - 2] + e[z - 6];
            k22 = e[z - 2] - e[z - 6];

            e[z] = y0 + y2;
            e[z - 2] = y0 - y2;

            k33 = e[z - 3] - e[z - 7];

            e[z - 4] = k00 + k33;
            e[z - 6] = k00 - k33;

            k11 = e[z - 1] - e[z - 5];
            y1 = e[z - 1] + e[z - 5];
            y3 = e[z - 3] + e[z - 7];

            e[z - 1] = y1 + y3;
            e[z - 3] = y1 - y3;
            e[z - 5] = k11 - k22;
            e[z - 7] = k11 + k22;
        }
    }
}
