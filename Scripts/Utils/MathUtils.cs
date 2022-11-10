using UnityEngine;

namespace MathUtils
{
    public class LineSegment
    {
        public Vector3 point1;
        public Vector3 point2;

        public LineSegment()
        {
            point1 = new Vector3();
            point2 = new Vector3();
        }

        public LineSegment(LineSegment aOther)
        {
            point1 = aOther.point1;
            point2 = aOther.point2;
        }

        public LineSegment(Vector3 aPoint1, Vector3 aPoint2)
        {
            point1 = aPoint1;
            point2 = aPoint2;
        }

        public Vector3[] GetLinesAsArray()
        {
            return new Vector3[2] { point1, point2 };
        }

        public float SquareDistance()
        {
            return (point2.x - point1.x) * (point2.x - point1.x) +
                   (point2.y - point1.y) * (point2.y - point1.y) +
                   (point2.z - point1.z) * (point2.z - point1.z);
        }

        public float Distance()
        {
            return Mathf.Sqrt(SquareDistance());
        }

        public float Slope()
        {
            return (point2.y - point1.y) / (point2.x - point1.x);
        }

        public bool ContainsPoint(Vector3 aPoint)
        {
            return aPoint == point1 || aPoint == point2;
        }

        public bool ContainsSegment(Vector3 aPoint1, Vector3 aPoint2)
        {
            return (aPoint1 == point1 || aPoint1 == point2)
                   && (aPoint2 == point1 || aPoint2 == point2);
        }

        public bool ContainsSegment(ref LineSegment aLineSegment)
        {
            return (aLineSegment.point1 == point1 || aLineSegment.point1 == point2)
                   && (aLineSegment.point2 == point1 || aLineSegment.point2 == point2);
        }

        public Vector3 VectorForm()
        {
            return point2 - point1;
        }

        public Vector3 PositiveVectorForm()
        {
            if (point1.x < point2.x)
            {
                return (point2 - point1);
            }
            else
            {
                return (point1 - point2);
            }
        }

        public void LocalizePoints(Transform aTransform)
        {
            point1 = aTransform.InverseTransformPoint(point1);
            point2 = aTransform.InverseTransformPoint(point2);
        }

        public Vector3 Normal()
        {
            Vector3 lineVector = point2 - point1;
            return new Vector3(-lineVector.y, lineVector.x, lineVector.z);
        }

        // return is between -PI and PI
        public float AngleRadians()
        {
            Vector3 vectorForm = VectorForm();
            float rawAngleR = Mathf.Atan2(vectorForm.y, vectorForm.x);
            return rawAngleR;
        }

        public void GetLineEquationValues(out float aSlope, out float aConstant)
        {
            aSlope = Slope();
            aConstant = point1.y - aSlope * point1.x;
        }

        public int WhichSideOfLineIsPointOn(Vector3 aPoint)
        {
            float slope = Slope();
            float yIntercept = point2.y - slope * point2.x;
            // sidecheck = mx - y + b
            float sideCheck = aPoint.x * slope - aPoint.y + yIntercept;
            sideCheck = MathUtil.RoundToDigits(sideCheck, ROUND_PLACE.HUNDREDS);

            if(sideCheck > 0)
            {
                return 1;
            }
            else if(sideCheck < 0)
            {
                return -1;
            }

            // Collinear
            return 0;
        }

        public float DistanceToPoint(Vector3 aPoint)
        {
            float distSquared = SquareDistance();
            if (distSquared == 0.0)
            {
                return Vector3.Distance(point1, aPoint);
            }
            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            float t = Vector3.Dot(aPoint - point1, point2 - point1) / distSquared;
            t = Mathf.Clamp(t, 0, 1);
            Vector3 projection = point1 + t * (point2 - point1);  // Projection falls on the segment
            return Vector3.Distance(aPoint, projection);
        }

        /**
         * <summary> 
         * moves the points of the line segment such that the refernce point becomes on the line segment
         * </summary>
         * <param name="aRefPoint">Point that is to be on the new line segment transformation.</param>
         */
        public void ReallignToPoint(Vector3 aRefPoint)
        {
            float distanceToPoint = DistanceToPoint(aRefPoint);

            if (distanceToPoint == 0.0f)
            {
                // point is already on line segment
                return;
            }

            int sideCheck = WhichSideOfLineIsPointOn(aRefPoint);
            Vector3 vectorForm = PositiveVectorForm();
            Vector3 perpendicularVector;
            if (sideCheck > 0)
            {
                perpendicularVector = new Vector3(vectorForm.y, -vectorForm.x, vectorForm.z);
            }
            else
            {
                perpendicularVector = new Vector3(-vectorForm.y, vectorForm.x, vectorForm.z);
            }
            Vector3 deltaChange = perpendicularVector.normalized * distanceToPoint;
            point1 += deltaChange;
            point2 += deltaChange;
        }

        /**
         * <summary> 
         * moves the points of the line segment in such a way that slope is maintained but positions are extended/receded
         * </summary>
         * <param name="aFactor">Value used to calculate the preserved distance of the line segment. Default is doubles the distance. 
         * Higher values will result in less distance gained, lower values will increase distance gain</param>
         */
        public void ExtendPoints(float aFactor = 2.0f)
        {
            point1.x = point1.x + (point1.x - point2.x) / aFactor;
            point1.y = point1.y + (point1.y - point2.y) / aFactor;
            point2.x = point2.x + (point2.x - point1.x) / aFactor;
            point2.y = point2.y + (point2.y - point1.y) / aFactor;
        }

        public Vector3 GetLerpPoint(float aLerpFactor)
        {
            return Vector3.Lerp(point1, point2, aLerpFactor);
        }
    }

    public class LeakyBucket
    {
        private readonly float m_max;
        private readonly float m_leakRate;
        private float m_current;

        public LeakyBucket(float aMax, float aRate)
        {
            m_max = aMax;
            m_leakRate = aRate;
            m_current = 0.0f;
        }

        public void AddValue(float aValue)
        {
            m_current += aValue;
            if(m_current >= m_max)
            {
                m_current = m_max;
                // TODO: setup some callback function here
            }
        }

        public void Update(float aDeltaTimeS)
        {
            m_current -= m_leakRate * aDeltaTimeS;
            if(m_current < 0.0f)
            {
                m_current = 0.0f;
            }
        }

        public void Reset()
        {
            m_current = 0f;
        }

        public float GetPercentFull()
        {
            return m_current / m_max;
        }

        public bool IsFull()
        {
            return GetPercentFull() == 1f;
        }

        public bool IsEmpty()
        {
            return GetPercentFull() == 0f;
        }
    }

    public enum AngleType
    {
        RADIANS,
        DEGREES
    }

    public class Angle
    {
        public AngleType type;
        public float value;

        public Angle()
        {
            type = AngleType.RADIANS;
            value = 0.0f;
        }

        public Angle(float aAngle)
        {
            type = AngleType.RADIANS;
            value = aAngle;
        }

        public Angle(float aAngle, AngleType aType)
        {
            type = aType;
            value = aAngle;
        }

        public void SetAngleType(AngleType aType)
        {
            if(type != aType)
            {
                if(aType == AngleType.DEGREES)
                {
                    value *= 180.0f / Mathf.PI;
                }
                else
                {
                    value *= Mathf.PI / 180.0f;
                }
                type = aType;
            }
        }

        public float GetValueAsType(AngleType aType)
        {
            if(aType == type)
            {
                return value;
            }
            else
            {
                if (aType == AngleType.DEGREES)
                {
                    return value * 180.0f / Mathf.PI;
                }
                else
                {
                    return value * Mathf.PI / 180.0f;
                }
            }
        }

        public void AddAngle(float aValue, AngleType aType)
        {
            if(aType == type)
            {
                value += aValue;
            }
            else
            {
                if(aType == AngleType.DEGREES)
                {
                    value += (aValue * Mathf.PI / 180.0f);
                }
                else
                {
                    value += (aValue * 180.0f / Mathf.PI);
                }
            }
            CheckBounds();
        }

        private void CheckBounds()
        {
            if(type == AngleType.RADIANS)
            {
                if (value < 0)
                {
                    value += MathUtil.TWOPI;
                }
                else if (value > MathUtil.TWOPI)
                {
                    value -= MathUtil.TWOPI;
                }
            }
            else
            {
                if (value < 0.0f)
                {
                    value += 360.0f;
                }
                else if (value > 360.0f)
                {
                    value -= 360.0f;
                }
            }
        }

        public static Angle operator+ (Angle lhs, Angle rhs)
        {
            float value = lhs.value + rhs.GetValueAsType(lhs.type);
            return new Angle(value, lhs.type);
        }
    }

    public static class MathUtil
    {
        public const float TWOPI = Mathf.PI * 2;

        // WARNING: subject to floating point error
        public static float GetFloatRemainder(float aValue)
        {
            return aValue % 1;
        }

        public static float CrossProduct2D(Vector2 point1, Vector2 point2)
        {
            return point1.x * point2.y - point1.y * point2.x;
        }

        public static float AngleBetweenRadians(Vector2 aVector1, Vector2 aVector2)
        {
            return Mathf.Acos(Vector2.Dot(aVector1, aVector2) / (aVector1.magnitude * aVector2.magnitude));
        }

        public static THREE_POINT_ORIENTATION GetPointsOrientation(Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3)
        {
            float val = (aPoint2.y - aPoint1.y) * (aPoint3.x - aPoint2.x) -
                      (aPoint2.x - aPoint1.x) * (aPoint3.y - aPoint2.y);

            if (val == 0) return THREE_POINT_ORIENTATION.COLINEAR;
            return (val > 0) ? THREE_POINT_ORIENTATION.CLOCKWISE : THREE_POINT_ORIENTATION.COUNTER_CLOCKWISE;
        }

        // https://github.com/pgkelley4/line-segments-intersect/blob/master/js/line-segments-intersect.js
        public static bool ComputeLinesIntersect(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 aIntersectionPoint)
        {
            aIntersectionPoint = new Vector2();
            Vector2 CminusA = C - A;
            Vector2 r = B - A;
            Vector2 s = D - C;

            float uNumerator = CrossProduct2D(CminusA, r);
            float denominator = CrossProduct2D(r, s);

            if (uNumerator == 0 && denominator == 0)
            {
                // Lines are collinear
                return false;
            }

            if (denominator == 0f)
            {
                // Lines are parallel
                return false;
            }

            float CminusAxs = CrossProduct2D(CminusA, s);
            float rxsr = 1f / denominator;
            float t = CminusAxs * rxsr;
            float u = uNumerator * rxsr;

            if ((t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f))
            {
                aIntersectionPoint = new Vector2(A.x + t * r.x, A.y + t * r.y);
                return true;
            }

            return false;
        }

        public static float RoundToDigits(float aValue, ROUND_PLACE aRoundTo)
        {
            float place = (float)aRoundTo;
            return Mathf.Round(aValue * place) / place;
        }

        public static bool GetCircleFromPoints(Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, out Vector2 aCenter, out float aRadius)
        {
            Vector2 midpoint1 = new Vector2((aPoint1.x + aPoint2.x) / 2, (aPoint1.y + aPoint2.y) / 2);
            Vector2 midpoint2 = new Vector2((aPoint2.x + aPoint3.x) / 2, (aPoint2.y + aPoint3.y) / 2);
            float slope1 = (aPoint2.y - aPoint1.y) / (aPoint2.x - aPoint1.x);
            float slope2 = (aPoint3.y - aPoint2.y) / (aPoint3.x - aPoint2.x);
            float perpSlope1 = -1 / slope1;
            float perpSlope2 = -1 / slope2;
            float foundX = (-midpoint1.y + midpoint2.y - (perpSlope1 * -midpoint1.x - perpSlope2 * -midpoint2.x)) / (perpSlope1 - perpSlope2);
            float foundY = perpSlope1 * (foundX - midpoint1.x) + midpoint1.y;

            aCenter = new Vector2(foundX, foundY);
            aRadius = Vector2.Distance(aPoint1, aCenter);
            return true;
        }

        public static double InCircle(Vector4 pa, Vector4 pb, Vector4 pc, Vector4 pd)
        {
            Vector2 circleCenter;
            float radius;
            if (GetCircleFromPoints(pa, pb, pc, out circleCenter, out radius))
            {
                return radius - Vector2.Distance(circleCenter, pd);
            }

            return 0;
        }

        public static bool InCircle(Vector4 aPoint, Vector3 aCenter, float aRadius)
        {
            float distance = Mathf.Abs(Vector3.Distance(aPoint, aCenter));
            return distance <= aRadius;
        }

        public static bool InCircleFast(Vector4 pa, Vector4 pb, Vector4 pc, Vector4 pd)
        {
            THREE_POINT_ORIENTATION orientation = GetPointsOrientation(pa, pb, pc);
            Vector4[] orientedPoints = new Vector4[3] { pa, pb, pc };
            if(orientation == THREE_POINT_ORIENTATION.CLOCKWISE)
            {
                orientedPoints = new Vector4[3] { pc, pb, pa };
            }

            // Don't know why this is here
            /*float row0x = orientedPoints[0].x - pd.x;
            float row0y = orientedPoints[0].y - pd.y; 
            float row1x = orientedPoints[1].x - pd.x;
            float row1y = orientedPoints[1].y - pd.y; 
            float row2x = orientedPoints[2].x - pd.x;
            float row2y = orientedPoints[2].y - pd.y;*/
            Matrix4x4 testMatrix = new Matrix4x4();
            testMatrix.SetRow(0, new Vector4(orientedPoints[0].x, orientedPoints[0].y, orientedPoints[0].x* orientedPoints[0].x + orientedPoints[0].y* orientedPoints[0].y, 1));
            testMatrix.SetRow(1, new Vector4(orientedPoints[1].x, orientedPoints[1].y, orientedPoints[1].x* orientedPoints[1].x + orientedPoints[1].y* orientedPoints[1].y, 1));
            testMatrix.SetRow(2, new Vector4(orientedPoints[2].x, orientedPoints[2].y, orientedPoints[2].x* orientedPoints[2].x + orientedPoints[2].y* orientedPoints[2].y, 1));
            testMatrix.SetRow(3, new Vector4(pd.x, pd.y, pd.x* pd.x + pd.y* pd.y, 1));

            return testMatrix.determinant > 0;
        }

        public static bool InBox(Vector2 aTopLeft, Vector2 aBottomRight, Vector2 aPoint)
        {
            LineSegment first = new LineSegment(new Vector3(aTopLeft.x, aTopLeft.y), new Vector3(aBottomRight.x, aTopLeft.y));
            LineSegment second = new LineSegment(new Vector3(aBottomRight.x, aTopLeft.y), new Vector3(aBottomRight.x, aBottomRight.y));
            LineSegment third = new LineSegment(new Vector3(aBottomRight.x, aBottomRight.y), new Vector3(aTopLeft.x, aBottomRight.y));
            LineSegment fourth = new LineSegment(new Vector3(aTopLeft.x, aBottomRight.y), new Vector3(aTopLeft.x, aTopLeft.y));

            int firstSide = first.WhichSideOfLineIsPointOn(aPoint);
            int secondSide = second.WhichSideOfLineIsPointOn(aPoint);
            int thirdSide = third.WhichSideOfLineIsPointOn(aPoint);
            int fourthSide = fourth.WhichSideOfLineIsPointOn(aPoint);

            if(firstSide > 0 
               && secondSide < 0
               && thirdSide < 0
               && fourthSide > 0)
            {
                return true;
            }

            return false;
        }
    }
}