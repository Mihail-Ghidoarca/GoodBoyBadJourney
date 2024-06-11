using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using QLearning;

[CustomEditor(typeof(OpenQLearningBrain))]
[CanEditMultipleObjects]
public class OpenQLearningBrainEditor : Editor
{
    
    //String for our brains directory
    SerializedProperty brainsSaveDirectory;

    //State and actions list
    SerializedProperty State_And_Actions_List;

    //Q-Learning Values
    SerializedProperty learningRate;
    SerializedProperty dicountFactor;
    
    //Replay batch samples
    SerializedProperty replayBatchSampleSize;
    SerializedProperty choicesBetweenBatchLearning;



    //Greedy-Epsilon Exploitation vs. Exploration

    SerializedProperty Greedy_Epsilon;
    SerializedProperty epsilon;

    SerializedProperty dynamicEpsilon;
    SerializedProperty epsilonDecayRate;

    SerializedProperty epsilonDynamicAnnealingDecay;
    SerializedProperty epsilonDynamicDecayTotalSessionValue;
    SerializedProperty epsilonStart;
    SerializedProperty epsilonEnd;

    SerializedProperty epsilonDynamicEpisodeDecay;
    SerializedProperty epsilonDynamicEpisode_InteractionsAmount;

    SerializedProperty epsilonDynamicExperienceDecay;
    SerializedProperty ageWeight_Epsilon;
    //SerializedProperty epsilon_Experienc_Exponential_Moving_Average_Decay;
    //SerializedProperty epsilon_Experienc_Exponential_Moving_Average_Reward;




    //Boltzmann Expolitation vs. Exploration

    SerializedProperty Boltzmann_Exploration;
    SerializedProperty temperature;

    SerializedProperty dynamicTemperature;

    SerializedProperty boltzmannTemperatureDynamicAnnealingDecay;
    SerializedProperty temperatureDynamicDecayTotalSessionValue;
    SerializedProperty temperatureStart;
    SerializedProperty temperatureEnd;

    SerializedProperty boltzmannTemperatureDynamicEpisodeDecay;
    SerializedProperty temperatureDynamic_EpisodeInteractionsAmount;
    SerializedProperty temperatureDecayRate;

    SerializedProperty boltzmannTemperatureDynamicExperienceDecay;
    //SerializedProperty rewardInfluenceOnDecay;
    //SerializedProperty qValueDIfferenceInfluenceOnDecay;
    SerializedProperty ageWeight_Temperature;


    //Replay Buffer Sample Selection
    SerializedProperty randomReplayBuffer;

    SerializedProperty stateStringReplayBuffer;
    SerializedProperty stateStringReplayBufferString;

    SerializedProperty singleAttributeReplayBuffer;
    SerializedProperty singleAttributeReplayBufferKey;
    SerializedProperty singleAttributeReplayBufferValue;


    //Replay Buffer Removal Policies
    SerializedProperty ReplayBufferSize;
    SerializedProperty amountOfExperiencesToRemove;

    SerializedProperty experienceRemovalIdSorted;

    SerializedProperty RB_RandomRemoval;

    SerializedProperty RB_FirstIn_FirstOut;

    SerializedProperty RB_LastIn_FirstOut;

    SerializedProperty RB_LastIn_LastOut;

    SerializedProperty RB_Prioritised_AttributeRemoval;
    SerializedProperty RB_Prioritised_AttributeRemovalKey;
    SerializedProperty RB_Prioritised_AttributeRemovalValue;



    //Sorted Bool
    SerializedProperty sortedOutput;


    //Shared Data Properties
    SerializedProperty sharedData;
    SerializedProperty sharedDataSource;

    bool learningVariables = true, exEpVariables, pL_ReplayBuffSelec, pL_ReplayBuffRemove;


    void OnEnable()
    {
        //OpenQLearningBrain script = (OpenQLearningBrain)target;

        brainsSaveDirectory = serializedObject.FindProperty("brainsSaveDirectory");
        sortedOutput = serializedObject.FindProperty("sortedOutput");

        State_And_Actions_List = serializedObject.FindProperty("State_And_Actions");

        learningRate = serializedObject.FindProperty("learningRate");
        dicountFactor = serializedObject.FindProperty("dicountFactor");

        replayBatchSampleSize = serializedObject.FindProperty("replayBatchSampleSize");
        choicesBetweenBatchLearning = serializedObject.FindProperty("choicesBetweenBatchLearning");



        //Greedy Epsilon
        Greedy_Epsilon = serializedObject.FindProperty("Greedy_Epsilon");
        epsilon = serializedObject.FindProperty("epsilon");

        dynamicEpsilon = serializedObject.FindProperty("dynamicEpsilon");
        epsilonDecayRate = serializedObject.FindProperty("epsilonDecayRate");

        epsilonDynamicAnnealingDecay = serializedObject.FindProperty("epsilonAnnealingDecay");
        epsilonDynamicDecayTotalSessionValue = serializedObject.FindProperty("epsilonDynamicDecayTotalSessionValue");
        epsilonStart = serializedObject.FindProperty("epsilonStart");
        epsilonEnd = serializedObject.FindProperty("epsilonEnd");

        epsilonDynamicEpisodeDecay = serializedObject.FindProperty("epsilonEpisodeDecay");
        epsilonDynamicEpisode_InteractionsAmount = serializedObject.FindProperty("epsilonDynamicEpisode_InteractionsAmount");

        epsilonDynamicExperienceDecay = serializedObject.FindProperty("epsilonExperienceFluctuations");
        ageWeight_Epsilon = serializedObject.FindProperty("ageWeight_Epsilon");


        //Boltzmann 
        Boltzmann_Exploration = serializedObject.FindProperty("Boltzmann_Exploration");
        temperature = serializedObject.FindProperty("temperature");

        dynamicTemperature = serializedObject.FindProperty("dynamicTemperature");

        boltzmannTemperatureDynamicAnnealingDecay = serializedObject.FindProperty("temperatureAnnealingDecay");
        temperatureDynamicDecayTotalSessionValue = serializedObject.FindProperty("temperatureDynamicDecayTotalSessionValue");
        temperatureStart = serializedObject.FindProperty("temperatureStart");
        temperatureEnd = serializedObject.FindProperty("temperatureEnd");

        boltzmannTemperatureDynamicEpisodeDecay = serializedObject.FindProperty("temperatureEpisodeDecay");
        temperatureDynamic_EpisodeInteractionsAmount = serializedObject.FindProperty("temperatureDynamic_EpisodeInteractionsAmount");
        temperatureDecayRate = serializedObject.FindProperty("temperatureDecayRate");

        boltzmannTemperatureDynamicExperienceDecay = serializedObject.FindProperty("temperatureExperienceFluctuations");
        ageWeight_Temperature = serializedObject.FindProperty("ageWeight_Temperature");

        //Replay Buffer Sample Selection
        randomReplayBuffer = serializedObject.FindProperty("randomReplayBuffer");

        stateStringReplayBuffer = serializedObject.FindProperty("stateStringReplayBuffer");
        stateStringReplayBufferString = serializedObject.FindProperty("stateStringReplayBufferString");

        singleAttributeReplayBuffer = serializedObject.FindProperty("singleAttributeReplayBuffer");
        singleAttributeReplayBufferKey = serializedObject.FindProperty("singleAttributeReplayBufferKey");
        singleAttributeReplayBufferValue = serializedObject.FindProperty("singleAttributeReplayBufferValue");


        //Replay Buffer Removal Policy
        ReplayBufferSize = serializedObject.FindProperty("ReplayBufferSize");
        amountOfExperiencesToRemove = serializedObject.FindProperty("amountOfExperiencesToRemove");

        experienceRemovalIdSorted = serializedObject.FindProperty("experienceRemovalIdSorted");

        RB_RandomRemoval = serializedObject.FindProperty("RB_RandomRemoval");

        RB_FirstIn_FirstOut = serializedObject.FindProperty("RB_FirstIn_FirstOut");

        RB_LastIn_FirstOut = serializedObject.FindProperty("RB_LastIn_FirstOut");

        RB_Prioritised_AttributeRemoval = serializedObject.FindProperty("RB_Prioritised_AttributeRemoval");
        RB_Prioritised_AttributeRemovalKey = serializedObject.FindProperty("RB_Prioritised_AttributeRemovalKey");
        RB_Prioritised_AttributeRemovalValue = serializedObject.FindProperty("RB_Prioritised_AttributeRemovalValue");


        //Shared Data Variables
        sharedData = serializedObject.FindProperty("sharedData");
        sharedDataSource = serializedObject.FindProperty("sharedDataSource");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((OpenQLearningBrain)target), typeof(OpenQLearningBrain), false);
        GUI.enabled = true;

        EditorGUILayout.BeginVertical("GroupBox");
        //GUILayout.Label("State And Actions", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(State_And_Actions_List);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();



        //Learning Variables
        
        EditorGUILayout.BeginVertical("GroupBox");
        //GUILayout.Label("Q Learning Variables", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        learningVariables = EditorGUILayout.Foldout(learningVariables, "Q Learning Variables", true, EditorStyles.foldoutHeader);
        if(learningVariables)
        {
            EditorGUILayout.PropertyField(learningRate);
            EditorGUILayout.Space(1f);
            EditorGUILayout.PropertyField(dicountFactor);
            EditorGUILayout.Space(1f);

            //GUILayout.Label("Replay Buffer Variables", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(replayBatchSampleSize);
            EditorGUILayout.Space(1f);
            EditorGUILayout.PropertyField(choicesBetweenBatchLearning);
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();



        //Exploration VS Exploitation
        
        EditorGUILayout.BeginVertical("GroupBox");
        //GUILayout.Label("Exploration Vs Exploitation", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        exEpVariables = EditorGUILayout.Foldout(exEpVariables, "Exploration Vs Exploitation", true, EditorStyles.foldoutHeader);
        if(exEpVariables)
        {
            //Greedy-Epsilon
            EditorGUILayout.PropertyField(Greedy_Epsilon);
            EditorGUILayout.Space(2.5f);
            if (Greedy_Epsilon.boolValue)
            {
                Boltzmann_Exploration.boolValue = false;
                EditorGUILayout.PropertyField(epsilon);

                EditorGUILayout.Space(2.5f);

                EditorGUILayout.PropertyField(dynamicEpsilon);

                EditorGUILayout.Space(2.5f);

                //EditorGUILayout.PropertyField(epsilon_Experienc_Exponential_Moving_Average_Reward);


                if (dynamicEpsilon.boolValue)
                {
                    EditorGUILayout.PropertyField(epsilonDecayRate, GUI.enabled = true);

                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(epsilonDynamicAnnealingDecay, GUI.enabled = true);
                    if (epsilonDynamicAnnealingDecay.boolValue)
                    {
                        epsilonDynamicEpisodeDecay.boolValue = false;
                        epsilonDynamicExperienceDecay.boolValue = false;
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(epsilonDynamicDecayTotalSessionValue, GUI.enabled = true);
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(epsilonStart, GUI.enabled = true);
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(epsilonEnd, GUI.enabled = true);

                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(epsilonDynamicEpisodeDecay, GUI.enabled = true);
                    if (epsilonDynamicEpisodeDecay.boolValue)
                    {
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(epsilonDynamicEpisode_InteractionsAmount, GUI.enabled = true);
                        epsilonDynamicAnnealingDecay.boolValue = false;
                        epsilonDynamicExperienceDecay.boolValue = false;
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(epsilonDynamicExperienceDecay, GUI.enabled = true);
                    if (epsilonDynamicExperienceDecay.boolValue)
                    {
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(ageWeight_Epsilon, GUI.enabled = true);

                        epsilonDynamicAnnealingDecay.boolValue = false;
                        epsilonDynamicEpisodeDecay.boolValue = false;
                    }

                    EditorGUILayout.EndVertical();

                }

            }


            //Boltzmann
            EditorGUILayout.PropertyField(Boltzmann_Exploration);
            EditorGUILayout.Space(2.5f);
            if (Boltzmann_Exploration.boolValue)
            {
                dynamicEpsilon.boolValue = false;
                EditorGUILayout.PropertyField(temperature);

                EditorGUILayout.Space(2.5f);

                EditorGUILayout.PropertyField(dynamicTemperature);

                EditorGUILayout.Space(2.5f);

                if (dynamicTemperature.boolValue)
                {
                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(boltzmannTemperatureDynamicAnnealingDecay, GUI.enabled = true);
                    if (boltzmannTemperatureDynamicAnnealingDecay.boolValue)
                    {
                        boltzmannTemperatureDynamicEpisodeDecay.boolValue = false;
                        boltzmannTemperatureDynamicExperienceDecay.boolValue = false;
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(temperatureDynamicDecayTotalSessionValue, GUI.enabled = true);
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(temperatureStart, GUI.enabled = true);
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(temperatureEnd, GUI.enabled = true);
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(boltzmannTemperatureDynamicEpisodeDecay, GUI.enabled = true);
                    if (boltzmannTemperatureDynamicEpisodeDecay.boolValue)
                    {
                        boltzmannTemperatureDynamicAnnealingDecay.boolValue = false;
                        boltzmannTemperatureDynamicExperienceDecay.boolValue = false;
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(temperatureDynamic_EpisodeInteractionsAmount, GUI.enabled = true);
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(temperatureDecayRate, GUI.enabled = true);
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.PropertyField(boltzmannTemperatureDynamicExperienceDecay);
                    if (boltzmannTemperatureDynamicExperienceDecay.boolValue)
                    {
                        boltzmannTemperatureDynamicAnnealingDecay.boolValue = false;
                        boltzmannTemperatureDynamicEpisodeDecay.boolValue = false;
                        EditorGUILayout.Space(1f);
                        EditorGUILayout.PropertyField(ageWeight_Temperature, GUI.enabled = true);
                    }
                    EditorGUILayout.EndVertical();

                }

            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();


        //Prioritised Learning Variables - Replay Buffer Sample Selection

        EditorGUILayout.BeginVertical("GroupBox");
        //GUILayout.Label("Prioritized Learning - Replay Buffer Sample Selection", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        pL_ReplayBuffSelec = EditorGUILayout.Foldout(pL_ReplayBuffSelec, "Prioritized Learning - Replay Buffer Sample Selection", true, EditorStyles.foldoutHeader);
        if(pL_ReplayBuffSelec)
        {
            EditorGUILayout.PropertyField(randomReplayBuffer);
            EditorGUILayout.Space(2.5f);
            if (randomReplayBuffer.boolValue)
            {
                singleAttributeReplayBuffer.boolValue = false;
                stateStringReplayBuffer.boolValue = false;
            }

            EditorGUILayout.PropertyField(stateStringReplayBuffer);
            EditorGUILayout.Space(2.5f);
            if (stateStringReplayBuffer.boolValue)
            {
                randomReplayBuffer.boolValue = false;
                singleAttributeReplayBuffer.boolValue = false;
                EditorGUILayout.PropertyField(stateStringReplayBufferString);
            }

            EditorGUILayout.PropertyField(singleAttributeReplayBuffer);
            EditorGUILayout.Space(2.5f);
            if (singleAttributeReplayBuffer.boolValue)
            {
                randomReplayBuffer.boolValue = false;
                stateStringReplayBuffer.boolValue = false;
                EditorGUILayout.PropertyField(singleAttributeReplayBufferKey);
                EditorGUILayout.PropertyField(singleAttributeReplayBufferValue);
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();


        //Replay Buffer Removal Policy
        EditorGUILayout.BeginVertical("GroupBox");
        //GUILayout.Label("Replay Buffer Sample Removal", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        pL_ReplayBuffRemove = EditorGUILayout.Foldout(pL_ReplayBuffRemove, "Replay Buffer Sample Removal", true, EditorStyles.foldoutHeader);
        if(pL_ReplayBuffRemove)
        {
            EditorGUILayout.PropertyField(ReplayBufferSize);
            EditorGUILayout.Space(1f);
            EditorGUILayout.PropertyField(amountOfExperiencesToRemove);
            EditorGUILayout.Space(1f);
            EditorGUILayout.PropertyField(experienceRemovalIdSorted);
            EditorGUILayout.Space(1f);

            EditorGUILayout.PropertyField(RB_RandomRemoval);
            EditorGUILayout.Space(2.5f);
            if (RB_RandomRemoval.boolValue)
            {
                RB_FirstIn_FirstOut.boolValue = false;
                RB_LastIn_FirstOut.boolValue = false;
                RB_Prioritised_AttributeRemoval.boolValue = false;
            }

            EditorGUILayout.PropertyField(RB_FirstIn_FirstOut);
            EditorGUILayout.Space(2.5f);
            if (RB_FirstIn_FirstOut.boolValue)
            {
                RB_RandomRemoval.boolValue = false;
                RB_LastIn_FirstOut.boolValue = false;
                RB_Prioritised_AttributeRemoval.boolValue = false;
            }

            EditorGUILayout.PropertyField(RB_LastIn_FirstOut);
            EditorGUILayout.Space(2.5f);
            if (RB_LastIn_FirstOut.boolValue)
            {
                RB_RandomRemoval.boolValue = false;
                RB_FirstIn_FirstOut.boolValue = false;
                RB_Prioritised_AttributeRemoval.boolValue = false;
            }

            EditorGUILayout.PropertyField(RB_Prioritised_AttributeRemoval);
            EditorGUILayout.Space(2.5f);
            if (RB_Prioritised_AttributeRemoval.boolValue)
            {
                RB_RandomRemoval.boolValue = false;
                RB_FirstIn_FirstOut.boolValue = false;
                RB_LastIn_FirstOut.boolValue = false;

                EditorGUILayout.PropertyField(RB_Prioritised_AttributeRemovalKey);
                EditorGUILayout.PropertyField(RB_Prioritised_AttributeRemovalValue);
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();


        //Saving Variables
        EditorGUILayout.BeginVertical("GroupBox");
        GUILayout.Label("Saving Variables", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(brainsSaveDirectory);
        EditorGUILayout.PropertyField(sortedOutput);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("GroupBox");
        GUILayout.Label("Shared Data Variables", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(sharedData);
        if(sharedData.boolValue)
        {
            EditorGUILayout.PropertyField(sharedDataSource);
        }

        EditorGUILayout.EndVertical();




        serializedObject.ApplyModifiedProperties();

    }


}
