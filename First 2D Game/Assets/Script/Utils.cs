using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GutsIMW.Utils
{
    public class UtilsClass
    {
        public static Vector2 GetRandomDir()
        {
            return new Vector2(UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f,1f)).normalized;
        }

    }
}
