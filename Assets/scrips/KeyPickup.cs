using UnityEngine;

/// <summary>
/// Put this on the KeyItem.
/// When the player walks into the invisible zone, the key disappears,
/// a pickup sound plays, and the Win screen is shown.
/// </summary>
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 60f;   // slow spin so the key is easy to spot

    private bool _collected;

    private void Update()
    {
        if (!_collected)
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        // Only the player counts. The player is the one that has PlayerHealth.
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        _collected = true;

        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayKey(transform.position);

        if (GameEndUI.Instance != null)
            GameEndUI.Instance.ShowWin();

        gameObject.SetActive(false);   // hide the key (the sound plays from its own object)
    }
}