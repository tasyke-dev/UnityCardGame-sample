using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private string userId;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    DatabaseReference databaseReference;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(InitializeFirebase());
        }
    }

    private IEnumerator InitializeFirebase()
    {
        yield return FirebaseApp.CheckAndFixDependenciesAsync();
        FirebaseApp app = FirebaseApp.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
        }

        isInitialized = true;
    }

    public void CheckLevelStatus(string chapter, string level, System.Action<bool> callback)
    {
        databaseReference.Child("users").Child(userId).Child("chapters").Child(chapter).Child(level).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                bool isCompleted = snapshot.Exists && (bool)snapshot.Value;
                callback.Invoke(isCompleted);
            }
        });
    }
}
