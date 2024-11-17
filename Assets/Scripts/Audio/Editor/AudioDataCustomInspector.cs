//FAIL_SAFE - If the Editor Folder is added to the Build, disable entire script in build
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioSystem
{
    [CustomPropertyDrawer(typeof(AudioData))]
    public class AudioDataCustomInspector : PropertyDrawer
    {
        private FloatField minDistance;
        private FloatField maxDistance;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //Creates the Visual Element
            VisualElement root = new();

            //Finds the Data
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Audio/Editor/AudioDataVisualTree.uxml");

            //Loads the data
            visualTree.CloneTree(root);

            //Sets Tooltips
            #region Set Tooltips
            root.Q<PropertyField>("AudioFile").tooltip = "The Audio File that will be played when called";
            root.Q<Slider>("Volume").tooltip = "Percentage oh how loud the volume should play.\nExample: 0.5 will play the AudioClip at half volume";
            root.Q<Slider>("Pitch").tooltip = "Modifies the pitch to sound higher or lower.\nDefault Pitch Value: 1";
            root.Q<Slider>("SpatialBlend").tooltip = "Sets how much the audio is affected by 3D spacial calculations (Example: Doppler Effect).\nA value of 0 makes the sound completely 2D.\\nA value of 1 makes the sound completely 3D";
            root.Q<Slider>("PanStereo").tooltip = "Adjusts if the Audio should be played on the left or right speaker. Most effective when wearing headphones\nIf Audio should be played equally on both sides, value = 0";
            root.Q<Slider>("ReverbZoneMix").tooltip = "";
            root.Q<Slider>("DopplerLevel").tooltip = "";
            root.Q<SliderInt>("Priority").tooltip = "";
            root.Q<SliderInt>("Spread").tooltip = "";
            root.Q<Toggle>("Loop").tooltip = "If the clip should play again immediately after the audio clip is finished";
            root.Q<Toggle>("PlayOnAwake").tooltip = "If the audio clip should play immediately";
            root.Q<Toggle>("FrequentSound").tooltip = "If the sound will be frequently played, used for optimization purposes (will not break game if set incorrectly. It may use more resources though)";
            root.Q<Toggle>("Mute").tooltip = "Mutes the audio clip";
            root.Q<Toggle>("BypassEffects").tooltip = "";
            root.Q<Toggle>("BypassListenerEffects").tooltip = "";
            root.Q<Toggle>("BypassReverbZones").tooltip = "";
            root.Q<Toggle>("IgnoreListenerVolume").tooltip = "";
            root.Q<Toggle>("IgnoreListenerPause").tooltip = "";
            root.Q<PropertyField>("AudioMixerGroup").tooltip = "The audio mixer used for this sound clip. This can remain empty";
            root.Q<FloatField>("MinDistance").tooltip = "Determines how close the sound can feel.\nIf distance is less than the minimum distance, the sound will stop becoming louder as the listener gets closer to the source";
            root.Q<FloatField>("MaxDistance").tooltip = "Determines how far the sound can feel.\nIf distance is more than the maximym distance, the sound will stop becoming quieter as the listener gets further from the source";
            root.Q<EnumField>("AudioRollOffMode").tooltip = "Defines how the audio is affected by Sound. Default: Linear\nLogarithmic - Use this mode when you want real-world rolloff\nLinear - Use this mode when you want to lower sound based on distance from the source\nCustom - Use this when you want to have Custom rolloff";
            #endregion

            //Sets boundaries for number fields
            minDistance = root.Q<FloatField>("MinDistance");
            maxDistance = root.Q<FloatField>("MaxDistance");

            //Returns the data
            return root;
        }
    }
}
#endif