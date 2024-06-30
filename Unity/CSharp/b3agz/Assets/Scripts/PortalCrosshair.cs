using UnityEngine;
using UnityEngine.UI;

namespace b3agz.Portals {

    public class PortalCrosshair : MonoBehaviour {

        [SerializeField] private PortalGun _gun;

        [SerializeField] Image _noPortal;

        [SerializeField] private Image _crosshairA;
        [SerializeField] private Image _crosshairB;
        [SerializeField] private Portal _portalA;
        [SerializeField] private Portal _portalB;

        private void Start() {

            Init();

        }

        private void LateUpdate() {
            UpdateCrosshairs();
        }

        public void Init() {
            _crosshairA.color = _portalA.Colour;
            _crosshairB.color = _portalB.Colour;
        }

        public void UpdateCrosshairs() {

            _noPortal.gameObject.SetActive(!_gun.ValidTarget);

            float portalATransparency = _portalA.Active ? 1f : 0.2f;
            Color a = _portalA.Colour;
            a.a = portalATransparency;
            _crosshairA.color = a;

            float portalBTransparency = _portalB.Active ? 1f : 0.2f;
            Color b = _portalB.Colour;
            b.a = portalBTransparency;
            _crosshairB.color = b;

        }

    }
}
