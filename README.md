1. Main Idea
This is a ASP.NET Core MVC web application for managing lecturer claims. The Lecturer Claim System is designed to make the monthly claim process smooth and easy. For lecturers, its more than just submitting a form, the system automatically handles the complex calculation for hours worked and pay rates, ensuring they get paid accurately and on time. The system also allows easy uploading of documents right alongside the claim. Each claim is then carefully reviewed by both a Programme Coordinator and an Academic Manager. The Programme Coordinator can forward the submitted claim to the manager or reject the claim. The Academic Manager can approve or reject the forwarded claim from the coordinator. Ultimately this systems creates a seamless experience that saves time, reduces frustration errors, and makes the entire process better for everyone involved. 

2. Roles and what they can do
a)Lecturer
-Logs in account (with username and password) created by HR.
-Can: Submit claims and track claim status
-Total payment is auto-calculated and cannot be edited manually

b)Programme Coordinator
-Logs in account (with username and password) created by HR.
-Can: Reviews Claims submitted by Lecturer an either forwards the claim to manager or rejects the claim
-Feedback for rejecting is required but feedback for forwarding is optional

c)Academic Manager
-Logs in account (with username and password) created by HR.
-Can: Verify Claims forwarded from Programme Coordinator and either approve or reject the claim
-Feedback for rejecting is required but feedback for approving is optional

d)HR
-Logs in account (with username and password) created by HR.
-Can: Process Approved Claims by generating the invoice. Manage Users in the system by updating their profiles or deleting them. View all invoices and mark them as paid. View/Have Access to lecturer, Programme Coordinator and Academic Manager Pages. 

3.Technologies Used
-ASP.NET Core MVC
-Entity Framework Core
-Microsoft SQL Server Management Studio for the database
-Bootstrap for basic stying of views

4. How to run the project
-Clone the Github repository link
-git clone https://github.com/ZodeaJ/ST10440733_PROG6212_POE.git
-cd prog-final-poe

Links:
Video Link: https://youtu.be/SuKORyNW8ec
PowerPoint Link: https://advtechonline-my.sharepoint.com/:p:/g/personal/st10440733_imconnect_edu_za/IQD4ek59OahhTISJSu0uTVeOAarvZJKo2OJ3tUOHw9CXntU?e=RJGRVn

Needed Information to log into roles:
HR: Username = "SK" and Password = "HR123"
Lecturer: Username = "GT" and Password = "lecturer123"
Coordinator: Username = "TK" and Password = "coordinator123"
Manager: Username = "IH" and Password = "manager123"
