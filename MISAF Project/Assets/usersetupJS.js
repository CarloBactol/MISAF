function openUserSetupEntry() {
    var myWindow = window.open("/Users_Setup/UserSetup_Entry.aspx", "_blank");
}

function SelectionUserList() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_SelectUserlist'));
    myModal.show();
}

function closeSelectionUserList() {
    document.getElementById("closemod_SelectUserlist").click();
}

function selectPrinting() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmPrint'));
    myModal.show();
}

function confirmUserSaveModal() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmsaveUser'));
    myModal.show();
}

function close_confirmUserSaveModal() {
    document.getElementById("btn_close1").click();
}


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// -- GRIDVIEWROW ONDATABOUND
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

function isNumberKey_decimal(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode == 46 && evt.srcElement.value.split('.').length > 1) {
        return false;
    }
    if (charCode != 46 && charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

function delete_textbox(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode == 46 && evt.srcElement.value.split('.').length > 1) {
        return false;
    }
    if (charCode != 46 && charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

//function myFunction() {
//    var x = document.getElementById("txt_ProdDesc");
//    x.value = x.value.toUpperCase();
//}

var SelectedRow = null;
var SelectedRowIndex = null;
var UpperBound = null;
var LowerBound = null;

window.onload = function () {
    UpperBound = parseInt('<%= this.gridView.Rows.Count %>') - 1;
    LowerBound = 0;
    SelectedRowIndex = -1;
    //SelectRow(0, SelectedRowIndex + 1);
}

function SelectRow(CurrentRow, RowIndex) {
    if (SelectedRow == CurrentRow || RowIndex > UpperBound || RowIndex < LowerBound)
        return;

    if (SelectedRow != null) {
        SelectedRow.style.backgroundColor = SelectedRow.originalBackgroundColor;
        SelectedRow.style.color = SelectedRow.originalForeColor;
    }

    if (CurrentRow != null) {
        CurrentRow.originalBackgroundColor = CurrentRow.style.backgroundColor;
        CurrentRow.originalForeColor = CurrentRow.style.color;
        CurrentRow.style.backgroundColor = '#F5F5DC';
        CurrentRow.style.color = 'Black';
    }

    SelectedRow = CurrentRow;
    SelectedRowIndex = RowIndex;
    setTimeout("SelectedRow.focus();", 0);
}

function SelectSibling(e) {
    var e = e ? e : window.event;
    var KeyCode = e.which ? e.which : e.keyCode;

    if (KeyCode == 40)
        SelectRow(SelectedRow.nextSibling, SelectedRowIndex + 1);
    else if (KeyCode == 38)
        SelectRow(SelectedRow.previousSibling, SelectedRowIndex - 1);
    return false;
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// -- GENERIC ITEM INVENTORY UPDATE
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// LAGAY NATIN SA PAGE NYA MISMO
//function closeItemListModal() {
//    document.getElementById("closeItemListModal").click();
//}

//function newItemListModal() {
//    var myModal = new bootstrap.Modal(document.getElementById('ItemListModal'));
//    myModal.show();
//    //document.getElementById('txt_ProdDesc').focus();
//    //$('#txt_ProdDesc').focus();
//    //myModal.addEventListener('shown.bs.modal', function (event) {
//    //    document.getElementById('txt_ProdDesc').focus();
//    //})
//    //$('#myModal').on('shown.bs.modal', function () {
//    //    $('#txt_ProdDesc').focus();
//    //});
//}

////function setFocus() {
////    //document.getElementById('txt_ProdDesc').focus();
////    document.getElementById('<%= txt_ProdDesc.ClientID %>').focus();
////}

//function closenewItemListModal() {
//    document.getElementById("closeItemListModal").click();
//}

function confirmationModal() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmsave'));
    myModal.show();
}


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// -- GENERIC ITEM RESERVATION
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
function selectItemList() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_ItemDisplaylist'));
    myModal.show();
}

function closemod_Itemlist() {
    document.getElementById("closemod_Itemlist").click();
}

function mod_ReferenceModal() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_ReferenceModal'));
    myModal.show();
}

function closeReferenceModal() {
    document.getElementById("closeReferenceModal").click();
}

function selectDRInvList() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_DRInvDisplaylist'));
    myModal.show();
}

function closemod_DRInvlist() {
    document.getElementById("closemod_DRInvlist").click();
}


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// -- GENERIC ITEM TRANSFER
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
function selectAvailableItemList() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_SearchAvailable'));
    myModal.show();
}

function selectReservationList() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_SearchReservationTable'));
    myModal.show();
}

function selectPrinting() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmPrint'));
    myModal.show();
}

function closeAvailableReservedModal() {
    $closemod_Available = $("#closemod_Available");
    if ($closemod_Available != null) {
        $closemod_Available.click();
    }

    closemod_Reserved = $("#closemod_Reserved");
    if (closemod_Reserved != null) {
        closemod_Reserved.click();
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// -- GENERIC ITEM SUMMARY
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
function selectMinimumInventory() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_minimum'));
    myModal.show();
}

function select_mod_WarehouseInventory() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_WarehouseInventory'));
    myModal.show();
}

function closeGenericInventorySummary() {
    closemod_Summary = $("#btn_close_summary");
    if (closemod_Summary != null) {
        closemod_Summary.click();
    }

    closemod_Summary = $("#btn_close_summary2");
    if (closemod_Summary != null) {
        closemod_Summary.click();
    }
    
}

//function selectReservationList() {
//    var myModal = new bootstrap.Modal(document.getElementById('mod_SearchReservationTable'));
//    myModal.show();
//}

//function closeAvailableReservedModal() {
//    $closemod_Available = $("#closemod_Available");
//    if ($closemod_Available != null) {
//        $closemod_Available.click();
//    }

//    closemod_Reserved = $("#closemod_Reserved");
//    if (closemod_Reserved != null) {
//        closemod_Reserved.click();
//    }
//}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

//function SwitchOnEnter(e, NextTextBox) {
//    var keynum
//    var keychar
//    var numcheck

//    if (window.event) // IE
//    {
//        keynum = e.keyCode
//    }
//    else if (e.which) // Netscape/Firefox/Opera
//    {
//        keynum = e.which
//    }
//    if (keynum == 13) {
//        //window.setTimeout(function () {
//        //    document.getElementById(NextTextBox).focus();
//        //}, 0);
//        //return false;
//        //document.getElementById(NextTextBox).focus();
//        document.onreadystatechange = function () {
//            if (document.readyState === "interactive" || document.readyState === "complete") {
//                window.setTimeout(function () {
//                    document.getElementById(NextTextBox).focus();
//                }, 0);
//            }
//        }
//        window.ready(function () {
//            alert('it works');
//        });
//    }
//}

//function EnterGrid(e) {
//    var e = e ? e : window.event;
//    var KeyCode = e.which ? e.which : e.keyCode;

//    if (KeyCode == 13)
//        SelectRow(SelectedRow.nextSibling, SelectedRowIndex + 1);
//    return false;
//}

function setFocus(Target) {
    var e = e ? e : window.event;
    var KeyCode = e.which ? e.which : e.keyCode;

    if (KeyCode == 13)
        document.getElementById(Target).focus();
    return false;
}

//function Keydown(txt) {
//    if (event.keyCode == 13) {
//        alert("F2 Button Clicked");
//        //add your operation here
//    }
//}

//document.Attributes.Add("onkeydown", "return handleEnter('" + btnItemSave.ClientID + "', event)");
//document.getElementById().attributes("onkeydown")

//var isCtrl = false;
//document.attachEvent('onclick', KeysShortcut);
//function KeysShortcut() {
//    if (event.keyCode == 16) {
//        isCtrl = true;
//    }
//    if (event.keyCode == 113 && isCtrl == true) {
//        document.getElementById(btnSave).click();
//    }
//}

function handleEnter(obj, e) {
    var e = e ? e : window.event;
    var KeyCode = e.which ? e.which : e.keyCode;
    if (keyCode == 13)
        document.getElementById("btnSave").click();
    return false;
}

var _oldColor;
function SetNewColor(source) {
    _oldColor = source.style.backgroundColor;
    //source.style.backgroundColor = '#00ff00';
    source.style.backgroundColor = _oldColor;
}

function SetOldColor(source)
{ source.style.backgroundColor = _oldColor; }

//var postbackUrl = '<%=ClientScript.GetPostBackEventReference(this.gv_ItemDisplayList, "Select$" + e.Row.RowIndex)%>';

//function TriggerServerSideClick(args) {
//    var url = String.format(postbackUrl, args);
//    eval(url);
//}

//function handleEnter(obj, event) {
//    var keyCode = event.keyCode ? event.keyCode : event.which ? event.which : event.charCode;
//    if (keyCode == 13) {
//        document.getElementById(obj).click();
//        return false;
//    }
//    else {
//        return true;
//    }
//}

//const input = document.querySelector('txt_ProdDesc');
//const log = document.getElementById('lblPLProdCode');

//input.addEventListener('keydown', logKey);

//function logKey(e) {
//    log.textContent += '${e.code}';
//}

//document.onkeydown = KeyDownHandler;

//document.onkeyup = KeyUpHandler;

//var CTRL = false;

//var SHIFT = false;

//var ALT = false;

//var CHAR_CODE = -1;

//function KeyDownHandler(e) {
//    var x = '';

//    if (document.all) {
//        var evnt = window.event;

//        x = evnt.keyCode;

//    }

//    else {
//        x = e.keyCode;

//    }

//    DetectKeys(x, true);

//    ShowReport();

//}

//function KeyUpHandler(e) {
//    var x = '';

//    if (document.all) {
//        var evnt = window.event;

//        x = evnt.keyCode;

//    }

//    else {
//        x = e.keyCode;

//    }

//    DetectKeys(x, false);

//    ShowReport();

//}

//function DetectKeys(KeyCode, IsKeyDown) {
//    if (KeyCode == '16') {
//        SHIFT = IsKeyDown;

//    }

//    else if (KeyCode == '17') {
//        CTRL = IsKeyDown;

//    }

//    else if (KeyCode == '18') {
//        ALT = IsKeyDown;

//    }

//    else {
//        if (IsKeyDown)

//            CHAR_CODE = KeyCode;

//        else

//            CHAR_CODE = -1;

//    }

//}

//function ShowReport() {
//    var TBReport = document.getElementById("txtEncoded");

//    var DIVCtrl = document.getElementById("txtEncoded");

//    var DIVShift = document.getElementById("txtEncoded");

//    var DIVAlt = document.getElementById("txtEncoded");

//    var DIVChar = document.getElementById("txtEncoded");

//    document.title = 'SHIFT: ' + SHIFT + ', CTRL: ' + CTRL + ', ALT: ' + ALT + ', Char code is: ' + CHAR_CODE;

//    TBReport.value = document.title;

//    if (SHIFT)

//        //DIVShift.style.visibility = "visible";
//        //DIVShift.textContent = "sample";
//    {
//        txt2 = document.getElementById('<%=txtEncoded.ClientId%>');
//        txt2.value = "hahaha";
//        document.getElementById("btnSave").click();
//    }

//    else

//        DIVShift.style.visibility = "hidden";

//    //if (ALT)

//    //    DIVAlt.style.visibility = "visible";

//    //else

//    //    DIVAlt.style.visibility = "hidden";

//    //if (CTRL)

//    //    DIVCtrl.style.visibility = "visible";

//    //else

//    //    DIVCtrl.style.visibility = "hidden";

//}

//var isCtrl = false;
//document.attachEvent('onclick', KeysShortcut);
//function KeysShortcut() {
//    if (event.keyCode == 16) {
//        isCtrl = true;
//    }
//    if (event.keyCode == 113 && isCtrl == true) {
//        document.getElementById("btnSave").click();
//    }
//}