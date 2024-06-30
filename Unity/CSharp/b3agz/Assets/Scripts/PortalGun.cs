using System;
using UnityEngine;

namespace b3agz.Portals {

    public class PortalGun : MonoBehaviour {

        [SerializeField] private GameObject _portalProjectile;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private Transform _muzzle;

        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private Portal _portal1;
        [SerializeField] private Portal _portal2;
        
        [SerializeField] private Vector3 _direction;
        [SerializeField] private bool _validTarget;
        public bool ValidTarget => _validTarget;

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update() {

            _direction = GetTargetDirection();
            if (_direction != Vector3.zero) {
                
                if (Input.GetMouseButtonDown(0)) {

                    GameObject projectile = Instantiate(_portalProjectile, _muzzle.position, _muzzle.rotation);
                    PortalBall ball = projectile.GetComponent<PortalBall>();

                    if (ball != null) {
                        projectile.transform.forward = _direction;
                        ball.SetPortal(_portal1);

                    } else {
                        throw new Exception("Attempted to spawn portal ball but prefab has no PortalBall component");
                    }

                }

                if (Input.GetMouseButtonDown(1)) {

                    GameObject projectile = Instantiate(_portalProjectile, _muzzle.position, _muzzle.rotation);
                    projectile.transform.forward = _direction;
                    PortalBall ball = projectile.GetComponent<PortalBall>();

                    if (ball != null) {

                        ball.SetPortal(_portal2);

                    } else {
                        throw new Exception("Attempted to spawn portal ball but prefab has no PortalBall component");
                    }

                }
            }

            if (Input.GetKeyDown(KeyCode.B)) {

                GameObject projectile = Instantiate(_ballPrefab, _muzzle.position, _muzzle.rotation);

            }

        }

        private Vector3 GetTargetDirection() {

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f, _layerMask)) {
                if (hit.transform.CompareTag("PortalSurface")) {
                    _validTarget = true;
                    return hit.point - transform.position;
                }
            }
            _validTarget = false;
            return Vector3.zero;

        }

    }

}
