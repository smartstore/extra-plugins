@setlocal enableextensions
@cd /d "%~dp0"

@echo off

set linkedplugins=SmartStore.AustraliaPost, SmartStore.AuthorizeNet, SmartStore.CanadaPost, SmartStore.Fedex, SmartStore.LivePersonChat, SmartStore.MailChimp, SmartStore.TwitterAuth, SmartStore.UPS, SmartStore.USPS, SmartStore.Verizon

set linksrc=%CD%\..\SmartStoreNET\src\Plugins
set linktarget=%CD%

FOR %%A IN (%linkedplugins%) DO (
	mklink /j "%linksrc%\%%A-sym" "%linktarget%\%%A"
)

pause
