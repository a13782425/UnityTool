using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class DelayNode : LogicNodeBase
    {
        public float time;

        private Coroutine _coroutine;
        public override bool OnExecute()
        {
            _coroutine = LogicRuntime.Instance.StartCoroutine(Test());
            return true;
        }

        private IEnumerator Test()
        {
            yield return new WaitForSeconds(time);
            IsComplete = true;
            _coroutine = null;
        }
        public override bool OnStop()
        {
            if (_coroutine!=null)
            {
                LogicRuntime.Instance.StopCoroutine(_coroutine);
            }
            return base.OnStop();
        }
    }
}
