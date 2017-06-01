using GrandTheftMultiplayer.Shared.Math;

namespace MpRpServer.Server.Jobs
{
    class JobsPos
    {
        public static Vector3 GetJobPosition(int jobId)
        {
            var coord = new Vector3(-1020.5, -2722.14, 13.8);
            if (jobId == 1)
            {
                return coord;
            }
            return coord;
        }
    }
}
