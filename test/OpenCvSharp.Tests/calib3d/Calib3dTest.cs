﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenCvSharp.Tests.Calib3D
{
    public class Calib3DTest : TestBase
    {
        /// <summary>
        /// https://stackoverflow.com/questions/25244603/opencvs-projectpoints-function
        /// </summary>
        [Fact]
        public void ProjectPointsTest()
        {
            var objectPointsArray = Generate3DPoints().ToArray();
            var objectPoints = new Mat(objectPointsArray.Length, 1, MatType.CV_64FC3, objectPointsArray);

            Mat intrisicMat = new Mat(3, 3, MatType.CV_64FC1);
            intrisicMat.Set<double>(0, 0, 1.6415318549788924e+003);
            intrisicMat.Set<double>(1, 0,0);
            intrisicMat.Set<double>(2, 0,0);
            intrisicMat.Set<double>(0, 1,0);
            intrisicMat.Set<double>(1, 1,1.7067753507885654e+003);
            intrisicMat.Set<double>(2, 1,0);
            intrisicMat.Set<double>(0, 2,5.3262822453148601e+002);
            intrisicMat.Set<double>(1, 2,3.8095355839052968e+002);
            intrisicMat.Set<double>(2, 2,1);

            Mat rVec = new Mat(3, 1, MatType.CV_64FC1);
            rVec.Set<double>(0, -3.9277902400761393e-002);
            rVec.Set<double>(1, 3.7803824407602084e-002);
            rVec.Set<double>(2, 2.6445674487856268e-002);

            Mat tVec = new Mat(3, 1, MatType.CV_64FC1);
            tVec.Set<double>(0, 2.1158489381208221e+000);
            tVec.Set<double>(1, -7.6847683212704716e+000);
            tVec.Set<double>(2, 2.6169795190294256e+001);

            Mat distCoeffs = new Mat(4, 1, MatType.CV_64FC1);  
            distCoeffs.Set<double>(0, 0);
            distCoeffs.Set<double>(1, 0);
            distCoeffs.Set<double>(2, 0);
            distCoeffs.Set<double>(3, 0);

            // without jacobian
            Mat projectedPoints = new Mat();
            Cv2.ProjectPoints(objectPoints, rVec, tVec, intrisicMat, distCoeffs, projectedPoints);

            // with jacobian
            Mat jacobian = new Mat();
            Cv2.ProjectPoints(objectPoints, rVec, tVec, intrisicMat, distCoeffs, projectedPoints, jacobian);

            objectPoints.Dispose();
            intrisicMat.Dispose();
            rVec.Dispose();
            tVec.Dispose();
            distCoeffs.Dispose();
            projectedPoints.Dispose();
            jacobian.Dispose();

            IEnumerable<Point3d> Generate3DPoints()
            {
                double x, y, z;

                x = .5; y = .5; z = -.5;
                yield return new Point3d(x, y, z);

                x = .5; y = .5; z = .5;
                yield return new Point3d(x, y, z);

                x = -.5; y = .5; z = .5;
                yield return new Point3d(x, y, z);

                x = -.5; y = .5; z = -.5;
                yield return new Point3d(x, y, z);

                x = .5; y = -.5; z = -.5;
                yield return new Point3d(x, y, z);

                x = -.5; y = -.5; z = -.5;
                yield return new Point3d(x, y, z);

                x = -.5; y = -.5; z = .5;
                yield return new Point3d(x, y, z);
            }
        }

        [Fact]
        public void SolvePnPTestByArray()
        {
            var rvec = new double[] { 0, 0, 0 };
            var tvec = new double[] { 0, 0, 0 };
            var cameraMatrix = new double[3, 3]
            {
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 }
            };
            var dist = new double[] { 0, 0, 0, 0, 0 };

            var objPts = new Point3f[]
            {
                new Point3f(0,0,1),
                new Point3f(1,0,1),
                new Point3f(0,1,1),
                new Point3f(1,1,1),
                new Point3f(1,0,2),
                new Point3f(0,1,2)
            };

            double[,] jacobian;
            Point2f[] imgPts;
            Cv2.ProjectPoints(objPts, rvec, tvec, cameraMatrix, dist, out imgPts, out jacobian);

            Cv2.SolvePnP(objPts, imgPts, cameraMatrix, dist, out rvec, out tvec);
        }
        
        [Fact]
        public void SolvePnPTestByMat()
        {
            var rvec = new double[] { 0, 0, 0 };
            var tvec = new double[] { 0, 0, 0 };
            var cameraMatrix = new double[3, 3]
            {
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 }
            };
            var dist = new double[] { 0, 0, 0, 0, 0 };

            var objPts = new Point3f[]
            {
                new Point3f(0,0,1),
                new Point3f(1,0,1),
                new Point3f(0,1,1),
                new Point3f(1,1,1),
                new Point3f(1,0,2),
                new Point3f(0,1,2)
            };

            double[,] jacobian;
            Point2f[] imgPts;
            Cv2.ProjectPoints(objPts, rvec, tvec, cameraMatrix, dist, out imgPts, out jacobian);

            using (var objPtsMat = new Mat(objPts.Length, 1, MatType.CV_32FC3))
            using (var imgPtsMat = new Mat(imgPts.Length, 1, MatType.CV_32FC2))
            using (var cameraMatrixMat = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (var distMat = Mat.Zeros(5, 0, MatType.CV_64FC1))
            using (var rvecMat = new Mat())
            using (var tvecMat = new Mat())
            {
                Cv2.SolvePnP(objPtsMat, imgPtsMat, cameraMatrixMat, distMat, rvecMat, tvecMat);
            }
        }
    }
}

