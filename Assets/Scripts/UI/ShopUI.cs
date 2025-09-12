using UnityEngine;
using Yarn.Unity;

public class ShopUI : MonoBehaviour
{
    [YarnCommand("OpenShop")]
    public void OpenShop() {
        Debug.Log("Shop Opened");
    }
}
