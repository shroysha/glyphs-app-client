using UnityEngine;

public class GlyphSphere : MonoBehaviour {

	private Glyph glyph;

	void Start() {
		this.gameObject.GetComponent<Renderer>().material.color = Color.blue;
		this.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
	}

	public void setGlyph(Glyph glyph) {
		this.glyph = glyph;
		this.gameObject.name = "Sphere " + glyph.GetGlyphId();
	}

	public void setSphereActive(bool isActive) {
		this.gameObject.SetActive (isActive);
	}

	public bool isSphereActive() {
		return this.gameObject.activeSelf;
	}

	public void setSphereColor(Color color) {
		this.gameObject.GetComponent<Renderer> ().material.color = color;
	}

	public Vector3 getGlyphSpherePosition() {
		return this.gameObject.transform.position;
	}

	public void repositionGlyphSphere(Vector3 newPosition) {
		this.gameObject.transform.position = newPosition;
	}

	public Glyph getGlyph() {
		return glyph;
	}
}
