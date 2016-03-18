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