using Burglary.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BurglaryPreUnityLoader
{
    internal class BurglaryMonoBehaviour : MonoBehaviour
    {
        void Update()
        {
            Utils.dispatcher.update();
        }
        void LateUpdate()
        {
            Utils.dispatcher.lateupdate();
        }
        void FixedUpdate()
        {
            Utils.dispatcher.fixedupdate();
        }
        void OnGUI()
        {
            Utils.dispatcher.OnGUI();
        }
    }
}
