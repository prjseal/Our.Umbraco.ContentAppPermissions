# Our.Umbraco.ContentAppPermissions

Manage who can see the content apps in your Umbraco site by changing an app setting in your web.config

You can create a group for each content app and assign the permission to that group or you can allow
an existing group to have permission to see the content app.


	<!-- Enter the Content app and allowed groups in this format: 
	ContentApp1Name[AllowedGroup1|AllowedGroup2],ContentApp2Name[AllowedGroup1] -->

<add key="ContentAppsRestrictedByGroup" value="Filter[Filter],Content[Administrators],Info[Sensitive data]"/>