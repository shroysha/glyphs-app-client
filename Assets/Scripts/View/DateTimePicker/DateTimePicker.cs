using System;
using UnityEngine;
using UnityEngine.UI;

public class DateTimePicker : MonoBehaviour {

	private static readonly int CONSTANT_DATETIME_SECONDS = 0;
	private static readonly string[] MERIDIEMS = {"AM", "PM"};
	private static readonly int NUM_DAYS_IN_WEEK = 7;
	private static readonly int MAX_ROW_COUNT = 6;

	private static readonly int MAX_DAY_BUTTONS = NUM_DAYS_IN_WEEK * MAX_ROW_COUNT; 

	private static readonly float PICKER_SCREEN_EDGE_PADDING = 16.0f;
	private static readonly float UI_PADDING = 8.0f;
	private static readonly float DAY_BUTTON_PADDING = 1.0f;
	private static readonly float TOTAL_DAY_BUTTON_WIDTH_PADDING = (float)(NUM_DAYS_IN_WEEK - 1) * DAY_BUTTON_PADDING;
	private static readonly float DAY_BUTTON_ASPECT_RATIO = 1.0f;

	private static readonly float TIME_VIEW_ASPECT_RATIO = 1.25f;

	public GameObject dayButtonPrefab;

	public RectTransform parentCanvasTransform;
	public RectTransform dateTimePickerTransform;

	public RectTransform buttonTabPanelTransform;
	public Button dateViewButton, timeViewButton;
	public RectTransform bottomToolbarTransform;
	public Button nowButton, confirmButton;
	public Text selectedDateText;

	public GameObject dateSelectionPanelObject;
	public GameObject timeSelectionPanelObject;

	public RectTransform monthSelectionPanelTransform;
	public Text dateViewMonthText, dateViewYearText;
	public Button previousMonthButton, nextMonthButton;
	public RectTransform daySelectionPanelTransform;
	public GridLayoutGroup daySelectionPanelGridLayout;
	public ToggleGroup dayButtonToggleGroup;

	public InputField hourField, minuteField, meridiemField;
	public Button upHourButton, downHourButton;
	public Button upMinuteButton, downMinuteButton;
	public Button amMeridiemButton, pmMeridiemButton;

	private DateTime selectedTime = DateTime.Now;

	private float datePickerWidth, datePickerHeight;
	private float timePickerWidth, timePickerHeight;

	private DayButton[] dayButtons = new DayButton[MAX_DAY_BUTTONS];

	private DateTime dateViewSelection;

	// Use this for initialization
	void Start () {
		selectedTime = selectedTime.AddSeconds (-selectedTime.Second);

		calculateConstraints ();
		instantiateDayButtons ();

		addButtonListeners ();

		switchToDateView ();
	}

	private void calculateConstraints() {
		calculateDateViewConstraints ();
		calculateTimeViewConstraints ();
	}

	private void calculateDateViewConstraints() {
		float maxWidth = parentCanvasTransform.sizeDelta.x - (PICKER_SCREEN_EDGE_PADDING * 2.0f);
		float maxHeight = parentCanvasTransform.sizeDelta.y / 1.5f;

		float maxWidthForDayButtons = maxWidth - UI_PADDING * 4.0f;
		float dayButtonWidthForMaxWidth = (maxWidthForDayButtons - TOTAL_DAY_BUTTON_WIDTH_PADDING) / (float)(NUM_DAYS_IN_WEEK);

		float heightUIElementsConsume = buttonTabPanelTransform.sizeDelta.y + monthSelectionPanelTransform.sizeDelta.y + bottomToolbarTransform.sizeDelta.y - UI_PADDING;

		float totalDayButtonHeightPadding = DAY_BUTTON_PADDING * (float)(MAX_ROW_COUNT - 1);
		float dayButtonHeightForMaxHeight = (maxHeight - heightUIElementsConsume - totalDayButtonHeightPadding) / (float)MAX_ROW_COUNT;
		float dayButtonWidthForMaxHeight = dayButtonHeightForMaxHeight / DAY_BUTTON_ASPECT_RATIO;

		float dayButtonWidth = Mathf.Min (dayButtonWidthForMaxWidth, dayButtonWidthForMaxHeight);
		float dayButtonHeight = dayButtonWidth * DAY_BUTTON_ASPECT_RATIO;

		daySelectionPanelGridLayout.cellSize = new Vector2 (dayButtonWidth, dayButtonHeight);
		float daySelectionPanelWidth = (dayButtonWidth * (float)NUM_DAYS_IN_WEEK) + TOTAL_DAY_BUTTON_WIDTH_PADDING + (UI_PADDING * 2.0f);
		float daySelectionPanelHeight = (dayButtonHeight * (float)MAX_ROW_COUNT) + totalDayButtonHeightPadding;

		datePickerWidth = daySelectionPanelWidth + (UI_PADDING * 2.0f);
		datePickerHeight =  daySelectionPanelHeight + heightUIElementsConsume;
	}

	private void calculateTimeViewConstraints() {
		float maxWidth = parentCanvasTransform.sizeDelta.x - (PICKER_SCREEN_EDGE_PADDING * 2.0f);
		float maxHeight = parentCanvasTransform.sizeDelta.y / 2.5f;

		float maxWidthBasedOnHeight = maxHeight * TIME_VIEW_ASPECT_RATIO;
		 
		timePickerWidth = Mathf.Min (maxWidth, maxWidthBasedOnHeight);
		timePickerHeight = timePickerWidth / TIME_VIEW_ASPECT_RATIO;
	}

	private void instantiateDayButtons() {
		for (int i = 0; i < dayButtons.Length; i++) {
			GameObject go = Instantiate (dayButtonPrefab);
			dayButtons [i] = go.GetComponent<DayButton> ();
			applyDayButtonConstants (dayButtons [i]);
		}
	}

	private void applyDayButtonConstants(DayButton button) {
		button.dayButtonTransform.SetParent (daySelectionPanelTransform);
		button.dayButtonTransform.localScale = Vector3.one;

		button.setDateTimePickerParent (this);

		Vector3 pos = button.dayButtonTransform.localPosition;
		pos.z = 0.0f;
		button.dayButtonTransform.localPosition = pos;

		dayButtonToggleGroup.RegisterToggle (button.dayButton);
		button.dayButton.group = dayButtonToggleGroup;
	}

	private void addButtonListeners() {
		dateViewButton.onClick.AddListener (switchToDateView);
		timeViewButton.onClick.AddListener (switchToTimeView);

		nowButton.onClick.AddListener (onNowButton);
		confirmButton.onClick.AddListener (onConfirmButton);

		previousMonthButton.onClick.AddListener (onPreviousMonthButton);
		nextMonthButton.onClick.AddListener (onNextMonthButton);

		upHourButton.onClick.AddListener (onDownHourButton);
		downHourButton.onClick.AddListener (onUpHourButton);
		upMinuteButton.onClick.AddListener (onDownMinuteButton);
		downMinuteButton.onClick.AddListener (onUpMinuteButton);
		amMeridiemButton.onClick.AddListener (onAMMeridiemButton);
		pmMeridiemButton.onClick.AddListener (onPMMeridiemButton);
	}

	private void switchToDateView() {
		timeSelectionPanelObject.SetActive (false);

		dateViewSelection = new DateTime (selectedTime.Year, selectedTime.Month, selectedTime.Day);

		resizeToDateView ();
		layoutDateView ();
		dateSelectionPanelObject.SetActive (true);
	}

	private void switchToTimeView() {
		dateSelectionPanelObject.SetActive (false);

		resizeToTimeView ();
		layoutTimeView ();
		timeSelectionPanelObject.SetActive (true);
	}

	private void resizeToDateView() {
		dateTimePickerTransform.sizeDelta = new Vector2 (datePickerWidth, datePickerHeight);
	}

	private void resizeToTimeView() {
		dateTimePickerTransform.sizeDelta = new Vector2 (timePickerWidth, timePickerHeight);
	}

	private void onNowButton() {
		selectedTime = DateTime.Now;
		layoutDateView ();
		layoutTimeView ();
	}

	private void onConfirmButton() {
		setVisible (false);
	}
		
	private void layoutDateView() {
		dateViewMonthText.text = dateViewSelection.ToString ("MMMM");
		dateViewYearText.text = dateViewSelection.ToString ("yyyy");

		reassignDayButtonDateTimes ();

		layoutConstantView ();
	}

	private void reassignDayButtonDateTimes() {
		// the sunday before the start of the month will always be our first day in the list
		DateTime firstOfMonth = new DateTime(dateViewSelection.Year, dateViewSelection.Month, 1);
		double daysUntilPreviousSunday = (double)(-(int)firstOfMonth.DayOfWeek);

		DateTime previousSunday = firstOfMonth.AddDays (daysUntilPreviousSunday);
		 
		// figure out if the 34th daybutton in array is in the month. if it is not, we can shift
		// the first sunday a week back for row spacing
		DateTime nValue = previousSunday.AddDays(34.0);
		bool nOutOfMonth = nValue.Month != dateViewSelection.Month; 
		if (nOutOfMonth) {
			previousSunday = previousSunday.AddDays (-(double)(NUM_DAYS_IN_WEEK));
		}

		double dayInset = 0.0f;
		foreach (DayButton daybutton in dayButtons) {
			DateTime buttonTime = previousSunday.AddDays (dayInset);
			daybutton.assignDateTime (buttonTime);
			dayInset++;

			bool buttonIsSelected = buttonTime.ToString ("yyyy-MM-dd").Equals (selectedTime.ToString ("yyyy-MM-dd"));
			daybutton.highlight (buttonIsSelected);

			bool outOfMonth = buttonTime.Month != dateViewSelection.Month;
			daybutton.outOfMonth (outOfMonth);
		}
	}

	private void layoutTimeView () {
		hourField.text = selectedTime.ToString("hh");
		minuteField.text = selectedTime.ToString ("mm");
		meridiemField.text = selectedTime.ToString ("tt");

		layoutConstantView ();
	}

	private void layoutConstantView() {
		selectedDateText.text = selectedTime.ToString("ddd, MMM dd hh:mm tt");
	}

	private void onNextMonthButton() {
		dateViewSelection = dateViewSelection.AddMonths (1);
		layoutDateView ();
	}

	private void onPreviousMonthButton() {
		DateTime newDateViewSelection = dateViewSelection.AddMonths (-1);

		// if the new year is greater, short circuit the or, otherwise compare year and month
		if ((newDateViewSelection.Year > DateTime.Now.Year) ||
				(newDateViewSelection.Year == DateTime.Now.Year &&
					newDateViewSelection.Month >= DateTime.Now.Month)) {
			dateViewSelection = newDateViewSelection;
			layoutDateView ();
		} 

	}

	public void onDayButton(DayButton selectedButton) {

		DateTime selectedButtonTime = selectedButton.getAssignedDateTime ();

		bool outOfMonth = selectedButtonTime.Month != dateViewSelection.Month;

		if (outOfMonth) {
			int yearDif = selectedButtonTime.Year - dateViewSelection.Year;
			int monthDif = selectedButtonTime.Month - dateViewSelection.Month;

			if (yearDif != 0) {
				monthDif += 12 * yearDif;
			}
			dateViewSelection = dateViewSelection.AddMonths (monthDif);
			layoutDateView ();
		} else {
			selectedTime = new DateTime (selectedButtonTime.Year, selectedButtonTime.Month, selectedButtonTime.Day,
				selectedTime.Hour, selectedTime.Minute, CONSTANT_DATETIME_SECONDS);

			switchToTimeView ();
		}

	}

	private void onUpHourButton() {
		assignDateTime(selectedTime.AddHours(1.0));
		layoutTimeView ();
	}

	private void onDownHourButton() {
		assignDateTime(selectedTime.AddHours(-1.0));
		layoutTimeView ();
	}

	private void onUpMinuteButton() {
		assignDateTime(selectedTime.AddMinutes(1.0));
		layoutTimeView ();
	}

	private void onDownMinuteButton() {
		assignDateTime(selectedTime.AddMinutes (-1.0));
		layoutTimeView ();
	}

	private void onAMMeridiemButton() {
		// if the selected time is pm and the am button is pushed
		if (selectedTime.ToString ("tt").Equals(MERIDIEMS[1])) {
			assignDateTime(selectedTime.AddHours (-12.0));
		}

		layoutTimeView ();
	}

	private void onPMMeridiemButton() {
		// if the selected time is am and the pm button is pushed
		if (selectedTime.ToString ("tt").Equals(MERIDIEMS[0])) {
			assignDateTime(selectedTime.AddHours (12.0));
		}

		layoutTimeView ();
	}

	/*
	 * Only supposed to be used with time selection
	 */
	private void assignDateTime(DateTime newDateTime) {
		if (newDateTime.CompareTo (DateTime.Now) >= 0) {
			selectedTime = newDateTime;
		} else {
			selectedTime = DateTime.Now;
			selectedTime = selectedTime.AddSeconds (-selectedTime.Second);
		}
	}

	public void setVisible(bool visible) {
		this.gameObject.SetActive (visible);
	}

	public DateTime getSelectedDateTime() {
		selectedTime = selectedTime.AddSeconds (-selectedTime.Second);

		return selectedTime;
	}

}
