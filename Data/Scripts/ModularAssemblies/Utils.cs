using VRageMath;

namespace Epstein_Fusion_DS
{
    public static class Utils
    {
        // TODO make this less inefficient.
        public static Matrix RotateMatrixAroundPoint(Matrix matrix, Vector3D point, Vector3D axis, double angleRadians)
        {
            // Translate the matrix to the origin (relative to the point of rotation)
            Matrix translationToOrigin = MatrixD.CreateTranslation(-point);
            Matrix translationBack = MatrixD.CreateTranslation(point);

            // Create the rotation matrix around the specified axis
            Matrix rotation = MatrixD.CreateFromAxisAngle(axis, angleRadians);

            // Combine the transformations
            Matrix transformedMatrix =  matrix * translationToOrigin * rotation * translationBack;
            //transformedMatrix = rotation * transformedMatrix;

            return transformedMatrix;
        }
    }
}
