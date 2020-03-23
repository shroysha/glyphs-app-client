using System.Collections.Generic;
using UnityEngine;

public class ScrollableList : MonoBehaviour {

	public GameObject scrollRectParent;
	public RectTransform scrollRectParentTransform;
	public GameObject allElementsPanel;
	public RectTransform allElementsPanelTransform;

	private List<ListGameObject> elements = new List<ListGameObject>();

	public static float TOP = 0.0f, SPACING = 0.0f, BOTTOM = 0.0f;

	public void addElement(ListGameObject toAdd) {
		elements.Add (toAdd);

		configureElementTransform (toAdd);

		resetContentSize ();
	}
		
	private void configureElementTransform(ListGameObject element) {
		element.GetListObjectTransform().SetParent (allElementsPanel.transform);

		Vector3 position = element.GetListObjectTransform().anchoredPosition3D;
		position.z = 0.0f;
		element.GetListObjectTransform().anchoredPosition3D = position;

		element.GetListObjectTransform().localScale = new Vector3 (1.0f, 1.0f, 1.0f);
	}

	private void resetContentSize() {
		float newHeight = TOP;
		for (int i = 0; i < elements.Count; i++) {
			newHeight += elements[i].GetListObjectTransform ().sizeDelta.y;

			if(!(i == elements.Count - 1)) {
				newHeight += SPACING;
			}
		}

		newHeight += BOTTOM;

		allElementsPanelTransform.sizeDelta = new Vector2 (allElementsPanelTransform.sizeDelta.x, newHeight);
	}

	public void removeAllElements() {

		foreach (ListGameObject element in elements) {
			Destroy (element.GetListObject ());
		}

		//allElementsPanelTransform.DetachChildren ();

		elements = new List<ListGameObject> ();
	}

	public void sortList() {
		allElementsPanelTransform.DetachChildren ();

		elements.Sort ();

		foreach (ListGameObject listObject in elements) {
			configureElementTransform (listObject);
		}
	}

	public interface ListGameObject {

		GameObject GetListObject ();
		RectTransform GetListObjectTransform();

	}
		
}
