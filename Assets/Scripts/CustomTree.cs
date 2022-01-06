using UnityEngine;
using UnityEditor;

public class CustomTree : MonoBehaviour
{
    public GameObject couronne;
    public GameObject fut;

    [SerializeField]
    [Range(.1f, 100f)]
    private float _circonference_cm = 1f;

    [SerializeField]
    [Range(.1f, 10f)]
    private float _hauteurTotale_m = 1f;

    [SerializeField]
    [Range(.1f, 10f)]
    private float _hauteurFut_m = 1f;

    [SerializeField]
    [Range(.2f, 10f)]
    private float _diametreCouronne_m = 1f;

    [SerializeField]
    [Range(.1f, 10f)]
    private float _rayonCouronne_m = 1f;

    [SerializeField]
    private string _essenceFrancais = "Arbre";

    private float _treeScale = 1000f;

    private GUIStyle style;

    private void Start()
    {
        style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
    }

    public void ReadValues(TreeProperties tp, float treeScale)
    {
        _circonference_cm = tp.circonference_cm;
        _hauteurTotale_m = tp.hauteurtotale_m;
        _hauteurFut_m = tp.hauteurfut_m;
        _diametreCouronne_m = tp.diametrecouronne_m;
        _rayonCouronne_m = tp.rayoncouronne_m;
        _essenceFrancais = tp.essencefrancais;

        _treeScale = treeScale;

        OnValidate();
    }

    private void OnValidate()
    {
        transform.localScale = Vector3.one * _treeScale;

        if (_hauteurFut_m > _hauteurTotale_m)
            _hauteurTotale_m = _hauteurFut_m;

        _rayonCouronne_m = _diametreCouronne_m / 2f;

        couronne.transform.localScale = new Vector3(_diametreCouronne_m, (_hauteurTotale_m / 2f) - (_hauteurFut_m / 2f), _diametreCouronne_m);
        fut.transform.localScale = new Vector3(_circonference_cm / 100f, _hauteurFut_m / 2f, _circonference_cm / 100f);

        fut.transform.position = new Vector3(fut.transform.position.x, (_hauteurFut_m / 2f) * transform.localScale.y, fut.transform.position.z);
        couronne.transform.position = new Vector3(couronne.transform.position.x, (_hauteurFut_m + couronne.transform.localScale.y) * transform.localScale.y, couronne.transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (style != null)
            Handles.Label(transform.position + Vector3.up * (_hauteurTotale_m * transform.localScale.y * 1.1f), _essenceFrancais, style);
        else
            Handles.Label(transform.position + Vector3.up * (_hauteurTotale_m * transform.localScale.y * 1.1f), _essenceFrancais);
    }
}
