using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModMenu.MonoBehaviours
{
    public class SliderMoveAudioFix : MonoBehaviour, IMoveHandler
    {
        public static EventReference onNavUpSound;
        public static EventReference onNavDownSound;
        public void OnMove(AxisEventData eventData)
        {
            // Fix the FMOD event not playing when moving on a slider
            switch (eventData.moveDir)
            {
                case MoveDirection.Up:
                    if (!onNavUpSound.IsNull)
                    {
                        RuntimeManager.PlayOneShot(onNavUpSound, default(Vector3));
                    }
                    return;
                case MoveDirection.Down:
                    if (!onNavDownSound.IsNull)
                    {
                        RuntimeManager.PlayOneShot(onNavDownSound, default(Vector3));
                    }
                    return;
            }
        }
    }
}