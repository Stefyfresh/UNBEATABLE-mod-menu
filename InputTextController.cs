// using UnityEngine;
// using TMPro;

// namespace ModMenu
// {
//     public class InputTextController : MonoBehaviour
//     {
//         private TMP_InputField input;
//         private Traverse m_PreventCallback;

//         private void Awake()
//         {
//             input = GetComponent<TMP_InputField>();
//             input?.onValueChanged.AddListener(OnValueChanged);
//             m_PreventCallback = Traverse.Create(input).Field("m_PreventCallback");
//         }

//         public void OnValueChanged(string text)
//         {
//             if (input != null)
//             {
//                 ModMenu.Logger.LogInfo("a");
//                 m_PreventCallback.SetValue(false);
//                 input.ForceLabelUpdate();
//                 m_PreventCallback.SetValue(true);
//                 input.textComponent.text = MenuBuilder.BeautifyString(text);
//             }
//         }
//     }
// }