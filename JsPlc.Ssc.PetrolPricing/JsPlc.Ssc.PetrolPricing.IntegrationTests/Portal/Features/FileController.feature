Feature: FileController
	In order to calculate tomorrow's petrol prices or see reports
	As Petrol Planner user
	I want to be able to upload Daily Price or Quarterly Site Data

Scenario: Upload Daily Price Data File
	Given I have valid Daily Price Data File for upload
	When I press Upload file button
	Then the test file should be visible in the list and its status should be Success
	And the test data should be deleted

Scenario: Upload Quarterly Site Data File
	Given I have valid Quarterly Data File for upload
	When I press Upload file button
	Then the test file should be visible in the list and its status should be Success

Scenario: Upload Quarterly Site Data File as Daily Price Data file
	Given I have valid Quarterly Data File for upload
	When I select Daily Price Date as File Type and press Upload file button
	Then Invalid Upload File Type error should appear