using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shadex
{
    /// <summary>
    /// Interpolate smoothing functions and any other static maths functionality.
    /// </summary>
    public class Numerology
    {
        #region "Interpolate"
        /**
         * Interpolation utility functions: easing, bezier, and catmull-rom.
         * Consider using Unity's Animation curve editor and AnimationCurve class
         * before scripting the desired behaviour using this utility.
         * Interpolation functionality available at different levels of abstraction.
         * Low level access via individual easing functions (ex.EaseInOutCirc),
         * Bezier(), and CatmullRom(). High level access using sequence generators,
         * NewEase(), NewBezier(), and NewCatmullRom().
         * Sequence generators are typically used as follows:
         *
         * IEnumerable<Vector3> sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration);
         * foreach (Vector3 newPoint in sequence) {
         *   transform.position = newPoint;
         *   yield return WaitForSeconds(1.0f);
         * }
         *
         * Or:
         *
         * IEnumerator<Vector3> sequence = Interpolate.New[Ease|Bezier|CatmulRom](configuration).GetEnumerator();
         * function Update() {
         *   if (sequence.MoveNext()) {
         *     transform.position = sequence.Current;
         *   }
         * }
         *
         * The low level functions work similarly to Unity's built in Lerp and it is
         * up to you to track and pass in elapsedTime and duration on every call. The
         * functions take this form (or the logical equivalent for Bezier() and CatmullRom()).
         *
         * transform.position = ease(start, distance, elapsedTime, duration);
         *
         * For convenience in configuration you can use the Ease(EaseType) function to
         * look up a concrete easing function:
         * 
         *  [SerializeField]
         *  Interpolate.EaseType easeType; // set using Unity's property inspector
         *  Interpolate.Function ease; // easing of a particular EaseType
         * function Awake() {
         *   ease = Interpolate.Ease(easeType);
         * }
         *
         * @author Fernando Zapata (fernando@cpudreams.com)
         * @Traduzione Andrea85cs (andrea85cs@dynematica.it)
         */


        /// <summary>
        /// Different methods of easing interpolation.
        /// </summary>
        public enum EaseType
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc
        }

        /// <summary>
        /// Sequence of elapsedTimes until elapsedTime is >= duration.
        /// </summary>
        /// <remarks>
        /// ElapsedTimes are calculated using the value of Time.deltatTime each time a value is requested.
        /// </remarks>
        /// <param name="v"></param>
        /// <returns></returns>
        static Vector3 Identity(Vector3 v)
        {
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static Vector3 TransformDotPosition(Transform t)
        {
            return t.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        static IEnumerable<float> NewTimer(float duration)
        {
            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                yield return elapsedTime;
                elapsedTime += Time.deltaTime;
                // make sure last value is never skipped
                if (elapsedTime >= duration)
                {
                    yield return elapsedTime;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public delegate Vector3 ToVector3<T>(T v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public delegate float Function(float a, float b, float c, float d);

        /// <summary>
        /// Generates sequence of integers from start to end (inclusive) one step at a time.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        static IEnumerable<float> NewCounter(int start, int end, int step)
        {
            for (int i = start; i <= end; i += step)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Returns sequence generator from start to end over duration using the
        /// given easing function.The sequence is generated as it is accessed
        /// using the Time.deltaTime to calculate the portion of duration that has elapsed.
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, float duration)
        {
            IEnumerable<float> timer = NewTimer(duration);
            return NewEase(ease, start, end, duration, timer);
        }


        /// <summary>
        /// Instead of easing based on time, generate n interpolated points (slices) between the start and end positions.
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, int slices)
        {
            IEnumerable<float> counter = NewCounter(0, slices + 1, 1);
            return NewEase(ease, start, end, slices + 1, counter);
        }


        /// <summary>
        /// Generic easing sequence generator used to implement the time and slice variants. Normally you would not use this function directly.
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="total"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        static IEnumerator NewEase(Function ease, Vector3 start, Vector3 end, float total, IEnumerable<float> driver)
        {
            Vector3 distance = end - start;
            foreach (float i in driver)
            {
                yield return Ease(ease, start, distance, i, total);
            }
        }


        /// <summary>
        /// Vector3 interpolation using given easing method. Easing is done independently on all three vector axis.
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static Vector3 Ease(Function ease, Vector3 start, Vector3 distance, float elapsedTime, float duration)
        {
            start.x = ease(start.x, distance.x, elapsedTime, duration);
            start.y = ease(start.y, distance.y, elapsedTime, duration);
            start.z = ease(start.z, distance.z, elapsedTime, duration);
            return start;
        }

        /// <summary>
        /// Returns the static method that implements the given easing type for scalars.
        /// </summary>
        /// <remarks>
        /// Use this method to easily switch between easing interpolation types.
        /// All easing methods clamp elapsedTime so that it is always <= duration.
        /// var ease = Interpolate.Ease(EaseType.EaseInQuad);
        /// i = ease(start, distance, elapsedTime, duration);
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Function Ease(EaseType type)
        {
            // Source Flash easing functions:
            // http://gizma.com/easing/
            // http://www.robertpenner.com/easing/easing_demo.html
            //
            // Changed to use more friendly variable names, that follow my Lerp
            // conventions:
            // start = b (start value)
            // distance = c (change in value)
            // elapsedTime = t (current time)
            // duration = d (time duration)

            Function f = null;
            switch (type)
            {
                case EaseType.Linear: f = Linear; break;
                case EaseType.EaseInQuad: f = EaseInQuad; break;
                case EaseType.EaseOutQuad: f = EaseOutQuad; break;
                case EaseType.EaseInOutQuad: f = EaseInOutQuad; break;
                case EaseType.EaseInCubic: f = EaseInCubic; break;
                case EaseType.EaseOutCubic: f = EaseOutCubic; break;
                case EaseType.EaseInOutCubic: f = EaseInOutCubic; break;
                case EaseType.EaseInQuart: f = EaseInQuart; break;
                case EaseType.EaseOutQuart: f = EaseOutQuart; break;
                case EaseType.EaseInOutQuart: f = EaseInOutQuart; break;
                case EaseType.EaseInQuint: f = EaseInQuint; break;
                case EaseType.EaseOutQuint: f = EaseOutQuint; break;
                case EaseType.EaseInOutQuint: f = EaseInOutQuint; break;
                case EaseType.EaseInSine: f = EaseInSine; break;
                case EaseType.EaseOutSine: f = EaseOutSine; break;
                case EaseType.EaseInOutSine: f = EaseInOutSine; break;
                case EaseType.EaseInExpo: f = EaseInExpo; break;
                case EaseType.EaseOutExpo: f = EaseOutExpo; break;
                case EaseType.EaseInOutExpo: f = EaseInOutExpo; break;
                case EaseType.EaseInCirc: f = EaseInCirc; break;
                case EaseType.EaseOutCirc: f = EaseOutCirc; break;
                case EaseType.EaseInOutCirc: f = EaseInOutCirc; break;
            }
            return f;
        }

        /// <summary>
        /// Returns sequence generator from the first node to the last node over
        /// </summary>
        /// <remarks>
        /// duration time using the points in-between the first and last node
        /// as control points of a bezier curve used to generate the interpolated points
        /// in the sequence.If there are no control points (ie.only two nodes, first
        /// and last) then this behaves exactly the same as NewEase(). In other words
        /// a zero-degree bezier spline curve is just the easing method. The sequence
        /// is generated as it is accessed using the Time.deltaTime to calculate the
        /// portion of duration that has elapsed.
        /// </remarks>
        /// <param name="ease"></param>
        /// <param name="nodes"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewBezier(Function ease, Transform[] nodes, float duration)
        {
            IEnumerable<float> timer = NewTimer(duration);
            return NewBezier<Transform>(ease, nodes, TransformDotPosition, duration, timer);
        }

        /// <summary>
        /// Instead of interpolating based on time, generate n interpolated points (slices) between the first and last node.
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="nodes"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewBezier(Function ease, Transform[] nodes, int slices)
        {
            IEnumerable<float> counter = NewCounter(0, slices + 1, 1);
            return NewBezier<Transform>(ease, nodes, TransformDotPosition, slices + 1, counter);
        }

        /// <summary>
        /// A Vector3[] variation of the Transform[] NewBezier() function. 
        /// </summary>
        /// <remarks>
        /// Same functionality but using Vector3s to define bezier curve.
        /// </remarks>
        /// <param name="ease"></param>
        /// <param name="points"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewBezier(Function ease, Vector3[] points, float duration)
        {
            IEnumerable<float> timer = NewTimer(duration);
            return NewBezier<Vector3>(ease, points, Identity, duration, timer);
        }

        /// <summary>
        /// A Vector3[] variation of the Transform[] NewBezier() function.
        /// </summary>
        /// <remarks>
        /// Same functionality but using Vector3s to define bezier curve.
        /// </remarks>
        /// <param name="ease"></param>
        /// <param name="points"></param>
        /// <param name="slices"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewBezier(Function ease, Vector3[] points, int slices)
        {
            IEnumerable<float> counter = NewCounter(0, slices + 1, 1);
            return NewBezier<Vector3>(ease, points, Identity, slices + 1, counter);
        }


        /// <summary>
        /// Generic bezier spline sequence generator used to implement the time and slice variants. Normally you would not use this function directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ease"></param>
        /// <param name="nodes"></param>
        /// <param name="toVector3"></param>
        /// <param name="maxStep"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        static IEnumerable<Vector3> NewBezier<T>(Function ease, IList nodes, ToVector3<T> toVector3, float maxStep, IEnumerable<float> steps)
        {
            // need at least two nodes to spline between
            if (nodes.Count >= 2)
            {
                // copy nodes array since Bezier is destructive
                Vector3[] points = new Vector3[nodes.Count];

                foreach (float step in steps)
                {
                    // re-initialize copy before each destructive call to Bezier
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        points[i] = toVector3((T)nodes[i]);
                    }
                    yield return Bezier(ease, points, step, maxStep);
                    // make sure last value is always generated
                }
            }
        }

        /// <summary>
        /// A Vector3 n-degree bezier spline.
        /// </summary>
        /// <remarks>
        /// WARNING: The points array is modified by Bezier.See NewBezier() for a
        /// safe and user friendly alternative.
        /// 
        /// You can pass zero control points, just the start and end points, for just
        /// plain easing.In other words a zero-degree bezier spline curve is just the
        /// easing method.
        /// </remarks>
        /// <param name="ease"></param>
        /// <param name="points"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static Vector3 Bezier(Function ease, Vector3[] points, float elapsedTime, float duration)
        {
            // Reference: http://ibiblio.org/e-notes/Splines/Bezier.htm
            // Interpolate the n starting points to generate the next j = (n - 1) points,
            // then interpolate those n - 1 points to generate the next n - 2 points,
            // continue this until we have generated the last point (n - (n - 1)), j = 1.
            // We store the next set of output points in the same array as the
            // input points used to generate them. This works because we store the
            // result in the slot of the input point that is no longer used for this
            // iteration.
            for (int j = points.Length - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i].x = ease(points[i].x, points[i + 1].x - points[i].x, elapsedTime, duration);
                    points[i].y = ease(points[i].y, points[i + 1].y - points[i].y, elapsedTime, duration);
                    points[i].z = ease(points[i].z, points[i + 1].z - points[i].z, elapsedTime, duration);
                }
            }
            return points[0];
        }


        /// <summary>
        /// Returns sequence generator from the first node, through each control point and to the last node.         
        /// </summary>
        /// <remarks>
        /// N points are generated between each node(slices) using Catmull-Rom.
        /// </remarks>
        /// <param name="nodes"></param>
        /// <param name="slices"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewCatmullRom(Transform[] nodes, int slices, bool loop)
        {
            return NewCatmullRom<Transform>(nodes, TransformDotPosition, slices, loop);
        }


        /// <summary>
        /// A Vector3[] variation of the Transform[] NewCatmullRom() function. 
        /// </summary>
        /// <remarks>
        /// Same functionality but using Vector3s to define curve.
        /// </remarks>
        /// <param name="points"></param>
        /// <param name="slices"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> NewCatmullRom(Vector3[] points, int slices, bool loop)
        {
            return NewCatmullRom<Vector3>(points, Identity, slices, loop);
        }


        /// <summary>
        /// Generic catmull-rom spline sequence generator used to implement the Vector3[] and Transform[] variants.
        /// </summary>
        /// <remarks>
        /// Normally you would not use this function directly.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes"></param>
        /// <param name="toVector3"></param>
        /// <param name="slices"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        static IEnumerable<Vector3> NewCatmullRom<T>(IList nodes, ToVector3<T> toVector3, int slices, bool loop)
        {
            // need at least two nodes to spline between
            if (nodes.Count >= 2)
            {

                // yield the first point explicitly, if looping the first point
                // will be generated again in the step for loop when interpolating
                // from last point back to the first point
                yield return toVector3((T)nodes[0]);

                int last = nodes.Count - 1;
                for (int current = 0; loop || current < last; current++)
                {
                    // wrap around when looping
                    if (loop && current > last)
                    {
                        current = 0;
                    }
                    // handle edge cases for looping and non-looping scenarios
                    // when looping we wrap around, when not looping use start for previous
                    // and end for next when you at the ends of the nodes array
                    int previous = (current == 0) ? ((loop) ? last : current) : current - 1;
                    int start = current;
                    int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
                    int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

                    // adding one guarantees yielding at least the end point
                    int stepCount = slices + 1;
                    for (int step = 1; step <= stepCount; step++)
                    {
                        yield return CatmullRom(toVector3((T)nodes[previous]),
                                         toVector3((T)nodes[start]),
                                         toVector3((T)nodes[end]),
                                         toVector3((T)nodes[next]),
                                         step, stepCount);
                    }
                }
            }
        }

        /// <summary>
        /// A Vector3 Catmull-Rom spline. 
        /// </summary>
        /// <remarks>
        /// Catmull-Rom splines are similar to bezier
        /// splines but have the useful property that the generated curve will go
        /// through each of the control points.
        ///
        /// NOTE: The NewCatmullRom() functions are an easier to use alternative to this
        /// raw Catmull-Rom implementation.
        /// </remarks>
        /// <param name="previous">the point just before the start point or the start point itself if no previous point is available.</param>
        /// <param name="start">generated when elapsedTime == 0.</param>
        /// <param name="end">generated when elapsedTime >= duration.</param>
        /// <param name="next">the point just after the end point or the end point itself if no next point is available</param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next,
                                    float elapsedTime, float duration)
        {
            // References used:
            // p.266 GemsV1
            //
            // tension is often set to 0.5 but you can use any reasonable value:
            // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
            //
            // bias and tension controls:
            // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

            float percentComplete = elapsedTime / duration;
            float percentCompleteSquared = percentComplete * percentComplete;
            float percentCompleteCubed = percentCompleteSquared * percentComplete;

            return previous * (-0.5f * percentCompleteCubed +
                                       percentCompleteSquared -
                                0.5f * percentComplete) +
                    start * (1.5f * percentCompleteCubed +
                               -2.5f * percentCompleteSquared + 1.0f) +
                    end * (-1.5f * percentCompleteCubed +
                                2.0f * percentCompleteSquared +
                                0.5f * percentComplete) +
                    next * (0.5f * percentCompleteCubed -
                                0.5f * percentCompleteSquared);
        }





        /// <summary>
        /// Linear interpolation (same as Mathf.Lerp).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float Linear(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime to be <= duration
            if (elapsedTime > duration) { elapsedTime = duration; }
            return distance * (elapsedTime / duration) + start;
        }


        /// <summary>
        /// Quadratic easing in - accelerating from zero velocity.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInQuad(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return distance * elapsedTime * elapsedTime + start;
        }


        /// <summary>
        /// quadratic easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutQuad(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return -distance * elapsedTime * (elapsedTime - 2) + start;
        }


        /// <summary>
        /// quadratic easing in/out - acceleration until halfway, then deceleration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutQuad(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
            if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime + start;
            elapsedTime--;
            return -distance / 2 * (elapsedTime * (elapsedTime - 2) - 1) + start;
        }


        /// <summary>
        /// cubic easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInCubic(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return distance * elapsedTime * elapsedTime * elapsedTime + start;
        }


        /// <summary>
        /// cubic easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutCubic(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            elapsedTime--;
            return distance * (elapsedTime * elapsedTime * elapsedTime + 1) + start;
        }


        /// <summary>
        /// cubic easing in/out - acceleration until halfway, then deceleration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutCubic(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
            if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime + start;
            elapsedTime -= 2;
            return distance / 2 * (elapsedTime * elapsedTime * elapsedTime + 2) + start;
        }


        /// <summary>
        /// quartic easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInQuart(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
        }

        /// <summary>
        /// quartic easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutQuart(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            elapsedTime--;
            return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1) + start;
        }


        /// <summary>
        /// quartic easing in/out - acceleration until halfway, then deceleration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutQuart(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
            if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
            elapsedTime -= 2;
            return -distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2) + start;
        }



        /// <summary>
        /// quintic easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInQuint(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
        }

        /// <summary>
        /// quintic easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutQuint(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            elapsedTime--;
            return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1) + start;
        }

        /// <summary>
        /// quintic easing in/out - acceleration until halfway, then deceleration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutQuint(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2f);
            if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
            elapsedTime -= 2;
            return distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2) + start;
        }


        /// <summary>
        /// sinusoidal easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInSine(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime to be <= duration
            if (elapsedTime > duration) { elapsedTime = duration; }
            return -distance * Mathf.Cos(elapsedTime / duration * (Mathf.PI / 2)) + distance + start;
        }


        /// <summary>
        /// sinusoidal easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutSine(float start, float distance, float elapsedTime, float duration)
        {
            if (elapsedTime > duration) { elapsedTime = duration; }
            return distance * Mathf.Sin(elapsedTime / duration * (Mathf.PI / 2)) + start;
        }


        /// <summary>
        /// sinusoidal easing in/out - accelerating until halfway, then decelerating
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutSine(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime to be <= duration
            if (elapsedTime > duration) { elapsedTime = duration; }
            return -distance / 2 * (Mathf.Cos(Mathf.PI * elapsedTime / duration) - 1) + start;
        }


        /// <summary>
        /// exponential easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInExpo(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime to be <= duration
            if (elapsedTime > duration) { elapsedTime = duration; }
            return distance * Mathf.Pow(2, 10 * (elapsedTime / duration - 1)) + start;
        }


        /// <summary>
        /// exponential easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutExpo(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime to be <= duration
            if (elapsedTime > duration) { elapsedTime = duration; }
            return distance * (-Mathf.Pow(2, -10 * elapsedTime / duration) + 1) + start;
        }


        /// <summary>
        /// exponential easing in/out - accelerating until halfway, then decelerating
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutExpo(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
            if (elapsedTime < 1) return distance / 2 * Mathf.Pow(2, 10 * (elapsedTime - 1)) + start;
            elapsedTime--;
            return distance / 2 * (-Mathf.Pow(2, -10 * elapsedTime) + 2) + start;
        }


        /// <summary>
        /// circular easing in - accelerating from zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInCirc(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            return -distance * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
        }

        /// <summary>
        /// circular easing out - decelerating to zero velocity
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseOutCirc(float start, float distance, float elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
            elapsedTime--;
            return distance * Mathf.Sqrt(1 - elapsedTime * elapsedTime) + start;
        }

        /// <summary>
        /// circular easing in/out - acceleration until halfway, then deceleration
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"></param>
        /// <param name="elapsedTime"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        static float EaseInOutCirc(float start, float distance, float
                             elapsedTime, float duration)
        {
            // clamp elapsedTime so that it cannot be greater than duration
            elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
            if (elapsedTime < 1) return -distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
            elapsedTime -= 2;
            return distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) + 1) + start;
        }
        #endregion
    }
}

/* *****************************************************************************************************************************
 * Copyright        : 2017 Shades of Insomnia
 * Founding Members : Charles Page (Shade)
 *                  : Rob Alexander (Insomnia)
 * License          : Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) https://creativecommons.org/licenses/by-sa/4.0/
 * Thanks to        : Fernando Zapata (fernando@cpudreams.com)
 *                    Traduzione Andrea85cs (andrea85cs@dynematica.it)
 *                    for the easing functions previously distributed as interpolate.cs
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
