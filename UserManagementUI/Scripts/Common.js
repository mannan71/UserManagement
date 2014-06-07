///-----------------Variables----------------
//For each control at runtime may be masterpage content place holder id is tagged. So it
//globally declared here
var vCtrlPrx = 'ctl00_contPlcHdrMasterHolder_';
//When calling a popup window then use this variable in the parameter so that all over
//the application a fixed size popup window will appear.
//For example: window.showModalDialog('XYZ',window, vPopupShape);
var vPopupShape = 'status:no;help:no;dialogWidth:470px;dialogHeight:490px;scrolling=no';
var vPopupShapeReportInfo = 'status:no;help:no;dialogWidth:700px;dialogHeight:490px;scrolling=no';
//When calling a popup window then use this variable to hold the caller window object
var vCallerWindowObject;
//When using urls in code it should add the following variable after controller name. It is necessary for URL routing
//For example: window.showModalDialog("../Common"+vRewriteURL+"/GetItemHierarchy, window, vPopupShape);
var vRewriteURL = "";
//When calling a popup window then use the following variables to callee window values
var vCalleeVar1;
var vCalleeVar2;
var vCalleeVar3;
var vCalleeVar4;
var vCalleeVar5;
var vConfirm;
///-----------------Functions----------------
//pMessage=For any customize message from caller
//pLevel=Folder level from the caller. It will need for custom popup
//pCustom=true for customization
function ShowConfirmation(pMessage, pLevel, pCustom) {
    if (pCustom == true) {
        //Code for customize popup ui and call according to pLevel and show message
        //according to pMessge
    }
    else {
        vConfirm = window.confirm(pMessage);
    }
} //End of function ShowConfirmation
//------------------------------------------------------------------------------
//pMessage=For any customize message from caller
//pLevel=Folder level from the caller. It will need for custom popup
//pCustom=true for customization
function ShowAlert(pMessage, pLevel, pCustom) {
    if (pCustom == true) {
        //Code for customize popup ui and call according to pLevel
    }
    else {
        window.alert(pMessage);        
    }
} //End of function ShowAlert
//------------------------------------------------------------------------------
//pMessage=For any customize message from caller
//pLevel=Folder level from the caller. It will need for custom popup
//pCustom=true for customization
function ShowAlertScript(pMessage, pLevel, pCustom) {
    return "<script type=\"text/javascript\" language=\"javascript\">ShowAlert(\"" + pMessage + "\"," + pLevel + ", " + pCustom + ")</script>;";
} //End of function ShowAlertScript
//------------------------------------------------------------------------------
//Pressing Enter will work as pressing Tab
function EnterNavigation(event) {
    //event.keyCode == 13 is for 'Enter' and event.keyCode == 9 for 'Tab'
    if (event.keyCode == 13) {
        window.event.keyCode = 9;
    }
}
//------------------------------------------------------------------------------
//Only takes number as input. call this function on the event 'onkeypress'
function InputOnlyNumber(pEvent) {
    pEvent = (pEvent) ? pEvent : window.event
    var charCode = (pEvent.which) ? pEvent.which : pEvent.keyCode
    if (charCode < 48 || charCode > 57) {
        pEvent.keyCode = 0;
    }
}
//------------------------------------------------------------------------------
//Only takes decimal number as input. call this function on the event 'onkeypress'
function InputOnlyDecimalNumber(event) 
{
    if (event.keyCode == 46) 
        return true;
    else {
        if(event.keyCode < 48 || event.keyCode > 57)
        {
            window.event.keyCode = 000;
            return false;
        }
        else
            return true;
    }    
}

function ReadOnlyText(event) 
{
    if (event.keyCode == 119) 
        return true;
    else {
        window.event.keyCode = 000;
        return false;
    }    
}

// It doesn't inform the user that he/she is typing too many characters.  
// It only prevents typing more than MaxLength.
// For example: Html.TextArea("Remarks", "", new { onkeypress = "return ImposeMaxLength(this, event);" })
function ImposeMaxLength(pObject, pEvent)
{
    vMaxLength = pObject.getAttribute("maxlength");
    if (pObject.value.length > vMaxLength-1) { 
        if (window.event) { 
            window.event.returnValue = null; 
        } else { 
            pEvent.cancelDefault;                 
            return false; 
        }         
    }
}

//----------------------------------------------------------------------------------------------
// Disable to click right button

var isnn, isie
if (navigator.appName == 'Microsoft Internet Explorer') //check the browser
{ isie = true }

if (navigator.appName == 'Netscape')
{ isnn = true }

function RightButton(e) //to trap right click button 
{
    if (isnn && (e.which == 3 || e.which == 2))
        return false;
    else if (isie && (event.button == 2 || event.button == 3)) {
        alert("Sorry, you do not have permission to right click on this page.");
        return false;
    }
    return true;
}
//----------------------------------------------------------------------------------------------
// Disable to press Ctrl & Alt key
function KeyCtrlAlt(k) {
    if (isie) {
        if (event.keyCode == 17 || event.keyCode == 18 || event.keyCode == 93) {
            alert("Sorry, you do not have permission to press this key.")
            return false;
        }
    }

    if (isnn) {
        alert("Sorry, you do not have permission to press this key.")
        return false;
    }
}
/*
if (document.layers) window.captureEvents(Event.KEYPRESS);
if (document.layers) window.captureEvents(Event.MOUSEDOWN);
if (document.layers) window.captureEvents(Event.MOUSEUP);
document.onkeydown = KeyCtrlAlt;
document.onmousedown = RightButton;
document.onmouseup = RightButton;
window.document.layers = RightButton;
*/