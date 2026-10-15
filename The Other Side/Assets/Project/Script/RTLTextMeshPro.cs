using TMPro;
using UnityEngine;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RTLTextMeshPro : TextMeshProUGUI
    {
        // protected override void Awake()
        // {
        //     base.Awake();
        //     isRightToLeftText = false;
        // }
        
        protected override void Awake()
        {
            base.Awake();
            isRightToLeftText = false;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            isRightToLeftText = false;
            text = PersianDisplayText.Prepare(text);
        }
#endif

        public void SetRtlText(string value)
        {
            isRightToLeftText = false;
            text = PersianDisplayText.Prepare(value);
        }
    }
