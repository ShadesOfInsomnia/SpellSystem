#define USE_INTERPOLATION         //Must have numerology.cs in project to use: Smooths out trails
//Make sure there is not another Interpolation.cs script in the project.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector;

namespace Shadex
{
    /// <summary>
    /// Applies a weapon trail to your equipped weapon.
    /// </summary>
    [vClassHeader("MELEE WEAPON TRAIL", iconName = "triggerIcon")]
    public class MeleeWeaponTrail : vMonoBehaviour
    {
        /// <summary>Enable emission.</summary>
        [SerializeField]
        bool _emit = true;
        public bool Emit { set { _emit = value; } }

        /// <summary>Time of emission.</summary>
        [SerializeField]
        float _emitTime = 0.00f;

        /// <summary>Emission material.</summary>
        [SerializeField]
        Material _material;

        /// <summary>Length of emission life.</summary>
        [SerializeField]
        float _lifeTime = 1.00f;

        /// <summary>Emission colours.</summary>
        [SerializeField]
        Color[] _colors;

        /// <summary>Size array.</summary>
        [SerializeField]
        float[] _sizes;

        /// <summary>Minimum vertex distance.</summary>
        [SerializeField]
        float _minVertexDistance = 0.10f;

        /// <summary>Maximum vertex distance.</summary>
        [SerializeField]
        float _maxVertexDistance = 10.00f;

        /// <summary>Minimum angle of emission.</summary>
        [SerializeField]
        float _maxAngle = 3.00f;

        /// <summary>Enable auto destruction of trail.</summary>
        [SerializeField]
        bool _autoDestruct = false;

#if USE_INTERPOLATION
        /// <summary>Sub division level aka the number of individual line trails created. the more subdivisions the smoother the trail.</summary>
        [SerializeField]
        [Tooltip("Higher numbers means more smooth")]
        int subdivisions = 32;  
#endif

        /// <summary>Base transform of the trail.</summary>
        [SerializeField]
        Transform _base;

        /// <summary>Tip transform of the trail.</summary>
        [SerializeField]
        Transform _tip;

        // internal
        List<Point> _points = new List<Point>();
#if USE_INTERPOLATION
        List<Point> _smoothedPoints = new List<Point>();
#endif
        GameObject _o;
        Mesh _trailMesh;
        Vector3 _lastPosition;
        Vector3 _lastCameraPosition1;
        Vector3 _lastCameraPosition2;
        bool _lastFrameEmit = true;

        /// <summary>
        /// Point internal class for the array of lines
        /// </summary>
        public class Point
        {
            public float timeCreated = 0.00f;
            public Vector3 basePosition;
            public Vector3 tipPosition;
            public bool lineBreak = false;
        }

        /// <summary>
        /// initialise on script enable
        /// </summary>
        void Start()
        {
            _lastPosition = transform.position;
            _o = new GameObject("Trail");
            _o.transform.parent = null;
            _o.transform.position = Vector3.zero;
            _o.transform.rotation = Quaternion.identity;
            _o.transform.localScale = Vector3.one;
            _o.AddComponent(typeof(MeshFilter));
            _o.AddComponent(typeof(MeshRenderer));
            _o.GetComponent<Renderer>().material = _material;

            _trailMesh = new Mesh();
            _trailMesh.name = name + "TrailMesh";
            _o.GetComponent<MeshFilter>().mesh = _trailMesh;
        }

        /// <summary>
        /// Destroy when the script is disabled.
        /// </summary>
        void OnDisable()
        {
            Destroy(_o);
        }

        /// <summary>
        /// Generate the trail per frame.
        /// </summary>
        void Update()
        {
            if (_emit && _emitTime != 0)
            {
                _emitTime -= Time.deltaTime;
                if (_emitTime == 0) _emitTime = -1;
                if (_emitTime < 0) _emit = false;
            }

            if (!_emit && _points.Count == 0 && _autoDestruct)
            {
                Destroy(_o);
                Destroy(gameObject);
            }

            // early out if there is no camera
            if (!Camera.main) return;

            // if we have moved enough, create a new vertex and make sure we rebuild the mesh
            float theDistance = (_lastPosition - transform.position).magnitude;
            if (_emit)
            {
                if (theDistance > _minVertexDistance)
                {
                    bool make = false;
                    if (_points.Count < 3)
                    {
                        make = true;
                    }
                    else
                    {
                        //Vector3 l1 = _points[_points.Count - 2].basePosition - _points[_points.Count - 3].basePosition;
                        //Vector3 l2 = _points[_points.Count - 1].basePosition - _points[_points.Count - 2].basePosition;
                        Vector3 l1 = _points[_points.Count - 2].tipPosition - _points[_points.Count - 3].tipPosition;
                        Vector3 l2 = _points[_points.Count - 1].tipPosition - _points[_points.Count - 2].tipPosition;
                        if (Vector3.Angle(l1, l2) > _maxAngle || theDistance > _maxVertexDistance) make = true;
                    }

                    if (make)
                    {
                        Point p = new Point();
                        p.basePosition = _base.position;
                        p.tipPosition = _tip.position;
                        p.timeCreated = Time.time;
                        _points.Add(p);
                        _lastPosition = transform.position;


#if USE_INTERPOLATION
                        if (_points.Count == 1)
                        {
                            _smoothedPoints.Add(p);
                        }
                        else if (_points.Count > 1)
                        {
                            // add 1+subdivisions for every possible pair in the _points
                            for (int n = 0; n < 1 + subdivisions; ++n)
                                _smoothedPoints.Add(p);
                        }

                        // we use 4 control points for the smoothing
                        if (_points.Count >= 4)
                        {
                            Vector3[] tipPoints = new Vector3[4];
                            tipPoints[0] = _points[_points.Count - 4].tipPosition;
                            tipPoints[1] = _points[_points.Count - 3].tipPosition;
                            tipPoints[2] = _points[_points.Count - 2].tipPosition;
                            tipPoints[3] = _points[_points.Count - 1].tipPosition;

                            //IEnumerable<Vector3> smoothTip = Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), tipPoints, subdivisions);
                            IEnumerable<Vector3> smoothTip = Numerology.NewCatmullRom(tipPoints, subdivisions, false);

                            Vector3[] basePoints = new Vector3[4];
                            basePoints[0] = _points[_points.Count - 4].basePosition;
                            basePoints[1] = _points[_points.Count - 3].basePosition;
                            basePoints[2] = _points[_points.Count - 2].basePosition;
                            basePoints[3] = _points[_points.Count - 1].basePosition;

                            //IEnumerable<Vector3> smoothBase = Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), basePoints, subdivisions);
                            IEnumerable<Vector3> smoothBase = Numerology.NewCatmullRom(basePoints, subdivisions, false);

                            List<Vector3> smoothTipList = new List<Vector3>(smoothTip);
                            List<Vector3> smoothBaseList = new List<Vector3>(smoothBase);

                            float firstTime = _points[_points.Count - 4].timeCreated;
                            float secondTime = _points[_points.Count - 1].timeCreated;

                            //Debug.Log(" smoothTipList.Count: " + smoothTipList.Count);

                            for (int n = 0; n < smoothTipList.Count; ++n)
                            {

                                int idx = _smoothedPoints.Count - (smoothTipList.Count - n);
                                // there are moments when the _smoothedPoints are lesser
                                // than what is required, when elements from it are removed
                                if (idx > -1 && idx < _smoothedPoints.Count)
                                {
                                    Point sp = new Point();
                                    sp.basePosition = smoothBaseList[n];
                                    sp.tipPosition = smoothTipList[n];
                                    sp.timeCreated = Mathf.Lerp(firstTime, secondTime, (float)n / smoothTipList.Count);
                                    _smoothedPoints[idx] = sp;
                                }
                                //else
                                //{
                                //	Debug.LogError(idx + "/" + _smoothedPoints.Count);
                                //}
                            }
                        }
#endif
                    }
                    else
                    {
                        _points[_points.Count - 1].basePosition = _base.position;
                        _points[_points.Count - 1].tipPosition = _tip.position;
                        //_points[_points.Count - 1].timeCreated = Time.time;

#if USE_INTERPOLATION
                        _smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
                        _smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
#endif
                    }
                }
                else
                {
                    if (_points.Count > 0)
                    {
                        _points[_points.Count - 1].basePosition = _base.position;
                        _points[_points.Count - 1].tipPosition = _tip.position;
                        //_points[_points.Count - 1].timeCreated = Time.time;
                    }

#if USE_INTERPOLATION
                    if (_smoothedPoints.Count > 0)
                    {
                        _smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
                        _smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
                    }
#endif
                }
            }

            if (!_emit && _lastFrameEmit && _points.Count > 0)
                _points[_points.Count - 1].lineBreak = true;

            _lastFrameEmit = _emit;




            List<Point> remove = new List<Point>();
            foreach (Point p in _points)
            {
                // cull old points first
                if (Time.time - p.timeCreated > _lifeTime)
                {
                    remove.Add(p);
                }
            }
            foreach (Point p in remove)
            {
                _points.Remove(p);
            }

#if USE_INTERPOLATION
            remove = new List<Point>();
            foreach (Point p in _smoothedPoints)
            {
                // cull old points first
                if (Time.time - p.timeCreated > _lifeTime)
                {
                    remove.Add(p);
                }
            }
            foreach (Point p in remove)
            {
                _smoothedPoints.Remove(p);
            }
#endif


#if USE_INTERPOLATION
            List<Point> pointsToUse = _smoothedPoints;
#else
        List<Point> pointsToUse = _points;
#endif

            if (pointsToUse.Count > 1)
            {
                Vector3[] newVertices = new Vector3[pointsToUse.Count * 2];
                Vector2[] newUV = new Vector2[pointsToUse.Count * 2];
                int[] newTriangles = new int[(pointsToUse.Count - 1) * 6];
                Color[] newColors = new Color[pointsToUse.Count * 2];

                for (int n = 0; n < pointsToUse.Count; ++n)
                {
                    Point p = pointsToUse[n];
                    float time = (Time.time - p.timeCreated) / _lifeTime;

                    Color color = Color.Lerp(Color.white, Color.clear, time);
                    if (_colors != null && _colors.Length > 0)
                    {
                        float colorTime = time * (_colors.Length - 1);
                        float min = Mathf.Floor(colorTime);
                        float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, _colors.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, colorTime);
                        if (min >= _colors.Length) min = _colors.Length - 1; if (min < 0) min = 0;
                        if (max >= _colors.Length) max = _colors.Length - 1; if (max < 0) max = 0;
                        color = Color.Lerp(_colors[(int)min], _colors[(int)max], lerp);
                    }

                    float size = 0f;
                    if (_sizes != null && _sizes.Length > 0)
                    {
                        float sizeTime = time * (_sizes.Length - 1);
                        float min = Mathf.Floor(sizeTime);
                        float max = Mathf.Clamp(Mathf.Ceil(sizeTime), 1, _sizes.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, sizeTime);
                        if (min >= _sizes.Length) min = _sizes.Length - 1; if (min < 0) min = 0;
                        if (max >= _sizes.Length) max = _sizes.Length - 1; if (max < 0) max = 0;
                        size = Mathf.Lerp(_sizes[(int)min], _sizes[(int)max], lerp);
                    }

                    Vector3 lineDirection = p.tipPosition - p.basePosition;

                    newVertices[n * 2] = p.basePosition - (lineDirection * (size * 0.5f));
                    newVertices[(n * 2) + 1] = p.tipPosition + (lineDirection * (size * 0.5f));

                    newColors[n * 2] = newColors[(n * 2) + 1] = color;

                    float uvRatio = (float)n / pointsToUse.Count;
                    newUV[n * 2] = new Vector2(uvRatio, 0);
                    newUV[(n * 2) + 1] = new Vector2(uvRatio, 1);

                    if (n > 0 /*&& !pointsToUse[n - 1].lineBreak*/)
                    {
                        newTriangles[(n - 1) * 6] = (n * 2) - 2;
                        newTriangles[((n - 1) * 6) + 1] = (n * 2) - 1;
                        newTriangles[((n - 1) * 6) + 2] = n * 2;

                        newTriangles[((n - 1) * 6) + 3] = (n * 2) + 1;
                        newTriangles[((n - 1) * 6) + 4] = n * 2;
                        newTriangles[((n - 1) * 6) + 5] = (n * 2) - 1;
                    }
                }

                _trailMesh.Clear();
                _trailMesh.vertices = newVertices;
                _trailMesh.colors = newColors;
                _trailMesh.uv = newUV;
                _trailMesh.triangles = newTriangles;

            }
            else
            {
                _trailMesh.Clear();

            }
        }
    }
}

/* *****************************************************************************************************************************
 * Copyright        : 2017 Shades of Insomnia
 * Founding Members : Charles Page (Shade)
 *                  : Rob Alexander (Insomnia)
 * License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
 * *****************************************************************************************************************************
 * You are free to:
 *     Share        : copy and redistribute the material in any medium or format.
 *     Adapt        : remix, transform, and build upon the material for any purpose, even commercially. 
 *     
 * The licensor cannot revoke these freedoms as long as you follow the license terms.
 * 
 * Under the following terms:
 *     Attribution  : You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may 
 *                    do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.
 *     ShareAlike   : If you remix, transform, or build upon the material, you must distribute your contributions under the same 
 *                    license as the original. 
 *                  
 * You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits. 
 * *****************************************************************************************************************************/
