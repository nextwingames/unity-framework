﻿using Nextwin.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Nextwin.UI
{
    /// <summary>
    /// UIManager의 base 추상 클래스, TEFrame과 TEDialog에 대응하는 enum을 정의해야함
    /// </summary>
    /// <typeparam name="TEFrame">Frame 식별자</typeparam>
    /// <typeparam name="TEDialog">Dialog 식별자</typeparam>
    public abstract class UIManagerBase<TEFrame, TEDialog> : Singleton<UIManagerBase<TEFrame, TEDialog>>
    {
        protected Dictionary<TEFrame, UIFrame<TEFrame>> _frames;
        protected Dictionary<TEDialog, UIDialog<TEDialog>> _dialogs;

        protected virtual void Start()
        {
            FindUIs(ref _frames);
            FindUIs(ref _dialogs);
        }

        public virtual UIFrame<TEFrame> GetFrame(TEFrame frameID)
        {
            if(!_frames.ContainsKey(frameID))
            {
                Debug.LogError($"There is no UI which id is {frameID}.");
                return null;
            }
            return _frames[frameID];
        }

        public virtual UIDialog<TEDialog> GetDialog(TEDialog dialogID)
        {
            if(!_dialogs.ContainsKey(dialogID))
            {
                Debug.LogError($"There is no UI which id is {dialogID}.");
                return null;
            }
            return _dialogs[dialogID];
        }

        /// <summary>
        /// 모든 UI를 찾아 Dictionary에 추가한 후 비활성화
        /// </summary>
        /// <typeparam name="T">UIBase의 하위 클래스</typeparam>
        /// <typeparam name="TEUI">UIBase의 ID를 지정할 enum</typeparam>
        /// <param name="dic">UI를 담을 Dictionary</param>
        protected virtual void FindUIs<T, TEUI>(ref Dictionary<TEUI, T> dic) where T : UIBase<TEUI>
        {
            dic = new Dictionary<TEUI, T>();

            foreach(T ui in FindObjectsOfType(typeof(T)))
            {
                if(IsDuplicatedID(ui.ID, dic))
                {
                    Debug.LogError($"Duplicated ID {ui.ID} of {ui.gameObject.name}.");
                    continue;
                }

                dic.Add(ui.ID, ui);
                ui.Show(false);
            }
        }

        protected virtual bool IsDuplicatedID<T, TEUI>(TEUI id, Dictionary<TEUI, T> dic)
        {
            if(!dic.ContainsKey(id))
            {
                return false;
            }

            return true;
        }
    }
}
