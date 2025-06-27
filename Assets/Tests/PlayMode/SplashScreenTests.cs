using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SplashScreenTests
{
    [UnityTest]
    public IEnumerator SplashScreen_AutoLoadsMainMenu()
    {
        SceneManager.LoadScene("SplashScreen");
        yield return null; // allow scene to load
        yield return new WaitForSeconds(2.5f);
        Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);
    }
}
