function openSRPModal() {
    var myModal = new bootstrap.Modal(document.getElementById('SRPmodal'));
    myModal.show();
}
function newProdListModal() {
    var myModal = new bootstrap.Modal(document.getElementById('ProdListModal'));
    myModal.show();
}
//function closeProdListModal() {
//    var myModal = new bootstrap.Modal(document.getElementById('ProdListModal'));
//    myModal.hide();
//}
//function closeProdListModal() {
//    var myModal = new bootstrap.Modal(document.getElementById('ProdListModal'));
//    myModal.classList.toggle("hide");
//}
function closeProdListModal() {
    var btn_closeProdListModal = document.getElementById("btn_closeProdListModal");
    if (btn_closeProdListModal) {
        btn_closeProdListModal.click();
    }

    var ProdListModal = document.getElementById("ProdListModal");
    if (ProdListModal) {
        ProdListModal.click();
    }
}

function newProdPackModal() {
    var myModal = new bootstrap.Modal(document.getElementById('ProdPackModal'));
    myModal.show();
}

function closeProdPackModal() {
    document.getElementById("closeProdPackModal1").click();
}

function newProdPictModal() {
    //document.getElementById("imgUpload").attributes("src", "/images/noimage.png");

    var myModal = new bootstrap.Modal(document.getElementById('ProdPicModal'));
    myModal.show();
}
function closeProdPicModal() {
    document.getElementById("closeProdPicModal").click();
}

function confirmationModal() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmsave'));
    myModal.show();
}

function closemod_confirmsave() {
    document.getElementById("btn_close1").click();
}

function newMainCatModal() {
    var myModal = new bootstrap.Modal(document.getElementById('MainCatModal'));
    myModal.show();
}

function newProdFamilyModal() {
    var myModal = new bootstrap.Modal(document.getElementById('ProdFamilyModal'));
    myModal.show();
}

function newProdModelModal() {
    var myModal = new bootstrap.Modal(document.getElementById('ProdModelModal'));
    myModal.show();
}

function newProdModelModal_Close() {
    document.getElementById("ProdModelModal_Close").click();
}

function msgModal() {
    var myModal = new bootstrap.Modal(document.getElementById('msgModal'));
    myModal.show();
}

function selectSKU() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_SKUlist'));
    myModal.show();
}

function closemod_SKUlist() {
    document.getElementById("closemod_SKUlist").click();
}

function selectFilterlist() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_Filterlist'));
    myModal.show();
}

function closeFilterlist() {
    document.getElementById("closemod_Filterlist").click();
}

function openModal() {
    //$('#mod_confirmvalidate_crd').modal('show');
    var myModal = new bootstrap.Modal(document.getElementById('filterModal'));
    //myModal.appendChild(document.getElementsByClassName('body'));
    myModal.show();
}

function closeFilterModal() {
    document.getElementById("filterClose").click();
}

function openProductEntry() {
    var myWindow = window.open("/Products/Product_Entry.aspx", "_blank");
}
function confDelModal() {
    var myModal = new bootstrap.Modal(document.getElementById('mod_confirmdelete'));
    myModal.show();
}

function Closepopup() {
    debugger;
    $('#ProdListModal').modal('hide');
}


//function submitForm() {
//    var formData = new FormData($('#imageForum')[0]);

//    $.ajax({
//        url: '/FileUpload',
//        type: 'POST',
//        data: formData,
//        async: false,
//        success: function (data) {
//            alert('posted')
//        },
//        cache: false,
//        contentType: false,
//        processData: false
//    });

//    return false;
//}

//function SaveAllDetails() {
//    if (document.getElementById("FileUploadMyImage").value != "") {
//        var file = document.getElementById('FileUploadMyImage').files[0];
//        var reader = new FileReader();
//        reader.readAsDataURL(file);
//        reader.onload = UpdateFiles;
//        alert(file);
//        alert(reader);
//    }
//    else {
//        alert('Please Choose An Image');
//    }
//}

//function UpdateFiles(evt) {
//    var result = evt.target.result;
//    var ImageSave = result.replace("data:image/jpeg;base64,", "");
//    var savobject = { 'savingvalues': ImageSave };
//    $.ajax({
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        url: "Default.aspx/SaveAllDetails",
//        data: JSON.stringify(savobject),
//        dataType: "json",
//        success: function () {
//            alert('SuccessFully Uploaded');
//        },
//        error: function () {
//            alert('Not Uploaded');
//        }
//    });
//}

//------------------------------------------------------------------------------------------------------------//
// ORDERING JS-SCRIPTS
// EDWIN LOPEZ - 07/20/2022
//------------------------------------------------------------------------------------------------------------//
//function showAlertMessage(msg, alertstyle) {
//    console.log("1");
//    setTimeout(hideAlertMessage(), 500);
//    console.log("2");
//    $("[id*=alerta]").each(function () {
//        console.log(msg);
//        $(this)
//            .addClass(alertstyle)
//            .children("span").html(msg)
//            .show().on("shown", function () {
//                window.setTimeout(function () {
//                    hideAlertMessage();
//                }, 500);
//            });

//        //window.setTimeout(function () {
//        //    $(this).hide();
//        //}, 5000);
//    });
//}

//function hideAlertMessage() {
//    // EMPTY REMOVE CLASS MUNA NATIN YUNG ALERT- (PARA FRESH)
//    $("[id*=alerta]").each(function () {
//        $(this)
//            .removeClass("alert-primary")
//            .removeClass("alert-secondary")
//            .removeClass("alert-success")
//            .removeClass("alert-danger")
//            .removeClass("alert-warning")
//            .removeClass("alert-info")
//            .removeClass("alert-light")
//            .removeClass("alert-dark")
//            .children("span").empty()
//            .hide();

//        //console.log("HIDE");
//    });
//}

function getDatafromCheckout() {
    $("[name*=lbl_data_temp]").val(sessionStorage.getItem("_data"));
    console.log($("[id*=lbl_data_temp]").val());
}
//------------------------------------------------------------------------------------------------------------//