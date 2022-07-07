#if UNITY_EDITOR
#if TERRAWORLD_PRO
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TerraUnity.Edittime
{
    public class ThreadHandler
    {
        public async Task WaitOnOtherThread(Action<Terrain, TTerrain> action, Terrain terrain, TTerrain tTerrainData, int miliSeconds = 3000)
        {
            await Task.Run(() => Delay(miliSeconds));
            action.Invoke(terrain, tTerrainData);
        }

        private void Delay (int miliSeconds)
        {
            Thread.Sleep(miliSeconds);
        }
    }
}
#endif
#endif

