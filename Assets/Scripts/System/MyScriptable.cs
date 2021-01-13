using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MyScriptable<T> : ScriptableObject where T : UnityEngine.Object 
{
    public static  string PATH = "Data";


    private static T _entity;
    public static T Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<T>(PATH + "/" + typeof(T));

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(PATH + "/" + typeof(T) + " not found");
                }
            }

            return _entity;
        }
    }
}


