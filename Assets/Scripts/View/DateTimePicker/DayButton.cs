using System;
using UnityEngine;
using UnityEngine.UI;

public class DayButton : MonoBehaviour {

	public RectTransform dayButtonTransform;
	public Toggle dayButton;
	public Text dayText;

	private DateTimePicker parent;
	private DateTime assignedDateTime;
	private ColorBlock normalColorBlock;
	private ColorBlock highlightedColorBlock;
	private ColorBlock outOfMonthColorBlock;

	public void setDateTimePickerParent(DateTimePicker picker) {
		parent = picker;
		dayButton.onValueChanged.AddListener (onDayButton);

		normalColorBlock = dayButton.colors;
		normalColorBlock.highlightedColor = normalColorBlock.normalColor;

		highlightedColorBlock = dayButton.colors;
		highlightedColorBlock.normalColor = highlightedColorBlock.highlightedColor;

		outOfMonthColorBlock = dayButton.colors;
		outOfMonthColorBlock.normalColor = Color.gray;
		outOfMonthColorBlock.highlightedColor = Color.gray;
		outOfMonthColorBlock.pressedColor = Color.white;
	}

	private void onDayButton(bool selected) {
		if (selected) {
			parent.onDayButton (this);
		}
	}

	public void assignDateTime(DateTime dt) {
		assignedDateTime = dt;

		layoutButton ();
	}

	private void layoutButton() {
		dayText.text = assignedDateTime.ToString ("dd");

		dayButton.interactable = !assignedDateIsPassed ();
	}

	public void highlight(bool highlight) {
		if (highlight) {
			dayButton.colors = highlightedColorBlock;
		} else {
			dayButton.colors = normalColorBlock;
		}
	}

	public void outOfMonth(bool outOfMonth) {
		if (outOfMonth) {
			dayButton.colors = outOfMonthColorBlock;
		}
	}

	public DateTime getAssignedDateTime() {
		return assignedDateTime;
	}
		
	private bool assignedDateIsPassed() {
		// if the year is in the future (greater), the date has not passed
		if (assignedDateTime.Year > DateTime.Now.Year) {
			return false;
			// we are now left with same or less year dates
			// if the year is in the past (less) the date has passed
		} else if (assignedDateTime.Year < DateTime.Now.Year) {
			return true;
			// we are not left with same year dates
		} else {
			if (assignedDateTime.Month > DateTime.Now.Month) {
				return false;
			} else if (assignedDateTime.Month < DateTime.Now.Month) {
				return true;
			} else {
				if (assignedDateTime.Day >= DateTime.Now.Day) {
					return false;
				} else {
					return true;
				}
			}
		}
	} 
}
