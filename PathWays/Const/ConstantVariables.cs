using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Const
{
    public static class ConstantVariables
    {
        public static RegionEndpoint region = RegionEndpoint.APSouth1;

        public static string CollectionId = "facerecognition_collection";

        public static string TableName = "face_recognition";

        public static string TableFieldName1 = "RekognitionId";

        public static string TableFieldName2 = "FullName";

        public static string BacketName = "person-names";

        public static string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TakeAndChooseImages");

        public static string awsAccessKeyId = "AKIAUFCGGQTZLJU7R26T";

        public static string awsSecretAccessKey = "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";

    }
}
