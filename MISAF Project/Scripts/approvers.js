// ================================ Variables =================================
let debounceTimer;
let isEmployeeNameSelected;


// ================================ Config & Routes =================================
const routes = {
    getApprover: '/Approver/GetSavedApprover',
    getLastRecordApprover: '/Approver/GetLastRecordApprover',
    getEmployeeById: '/Approver/GetEmployeeById',
    getEmployeeByName: '/Approver/GetEmployeeByName',
    getEmployeeByEmail: '/Approver/GetEmployeeByEmail',
    getSelectedEmail: '/Approver/GetSelectedEmail',
    getEmployeeByFullName: '/Approver/GetEmployeeByFullName',
    addApprover: '/Approver/AddApprover',
    editApprover: '/Approver/EditApprover',
    updateApprover: '/Approver/UpdateApprover',
    deleteApprover: '/Approver/DeleteApprover',
};

// ================================ Initialization ===================================
$(document).ready(function () {
    initApproverChosen();
    loadInitialTables();
    initApproverEvents();
    initToastr();
    initIDNoEvents();
    initNameEvents();
    initEmailEvents();
    
});

function initApproverChosen() {
    $('.chosen-select-single, .chosen-select').chosen({ width: '100%' });
}


function loadInitialTables() {
    $.get(routes.getApprover, updateApproverTable);
}

function loadLastRecordApprover() {
    $.get(routes.getLastRecordApprover, getLastRecord);
}


// ================================ Events ===================================

function initApproverEvents() {
    $('#btnAddApprover').on('click', () => openApproverModal('#approverModal', clearApproverModal));
    $('#btnClose').on('click', () => closeApproverModal('#approverModal', clearApproverModal));
    $('#btnSelectEmpName').on('click', () => saveSelectedEmpName());
    $('#employeeModal').on('hidden.bs.modal', () => isSelectedEmpName());
    $('#empName').on('change', () => onEmpNameChange());
    $('#empEmail').on('change', () => onSelectedEmailChange());
    
}

function initIDNoEvents() {

    // Run search on Enter
    $('#IDNo').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            onIDNoChange();
        }
    });

    // Run search on blur
    $('#IDNo').on('blur', function () {
        onIDNoChange();
    });

    // Run search after delay while typing
    //$('#IDNo').on('input', function () {
    //    clearTimeout(debounceTimer);
    //    debounceTimer = setTimeout(onIDNoChange, 900); // 500ms debounce
    //});


}

function initNameEvents() {

    // Run search on Enter
    $('#name').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            onNameChange();
        }
    });

    // Run search on blur
    $('#name').on('blur', function () {
        onNameChange();
    });
}

function initEmailEvents() {

    // Run search on Enter
    $('#emailCC').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            onEmailCCChange();
        }
    });

    // Run search on blur
    $('#emailCC').on('blur', function () {
        onEmailCCChange();
    });
}


// ================================ Approver Management ==============================
function updateApproverTable(data) {
    const table = $('#approverTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#approverTableBody').empty();
    $.each(data, function (i, item) {
        body.append(`
                <tr>
                    <td>${item.ID_No}</td>
                    <td>${item.Name}</td>
                    <td>${item.Endorser_Only}</td>
                    <td>${item.Email_CC != null ? item.Email_CC : 'No email address'}</td>
                    <td>${item.Active}</td>
                    <td>${item.MIS}</td>
                    <td>
                        <button onclick="editApprover(${item.Approver_ID}, event)" class="btn btn-info btn-sm"><i class="mdi mdi-pencil"></i></button>
                        <button onclick="deleteApprover(${item.Approver_ID}, event)" class="btn btn-danger btn-sm"><i class="mdi mdi-delete"></i></button>
                    </td>
                </tr>`);
    });
    table.DataTable({ paging: true, searching: true, ordering: true, lengthChange: true, lengthMenu: [[5, 10, 50, 100], [5, 10, 50, 100]] });
}

function getLastRecord(res) {
    $('#approverID').val(res.data.Approver_ID);
}

function saveApprover(event) {
    event.preventDefault();
    if (!validateApproverForm()) return;

    var token = $('input[name="__RequestVerificationToken"]').val();
    var endorserOnly = $('#endorserOnly').is(':checked');
    var active = $('#active').is(':checked');
    var mis = $('#mis').is(':checked');

    var approver = {
        Approver_ID: $('#approverID').val(),
        ID_No: $('#IDNo').val(),
        Name: $('#name').val(),
        Email_CC: $('#emailCC').val(),
        Endorser_Only: endorserOnly ? "Y" : "N",
        Active: active ? "Y" : "N",
        MIS: mis ? "Y" : "N",
        __RequestVerificationToken: token
    }

    var modal = $('#approverModalLabel').text();
    var add = modal.includes("Add");
    var edit = modal.includes("Edit");


    if (add) {
        $.post(routes.addApprover, approver, function (res) {

            if (!res.success) {
                toastr.warning(res.message);
                return;
            }

            toastr.success("Approver saved successfully.")
            $('#approverModal').modal('hide');
            updateApproverTable(res.data)
        }).fail(function () {
            toastr.error(res.message);
        });
    }

    if (edit) {
        $.post(routes.updateApprover, approver, function (res) {
            toastr.info("Approver updated successfully.")
            $('#approverModal').modal('hide');
            updateApproverTable(res.data)
        });
    }
}


function editApprover(index, event) {
    if (event) event.preventDefault();

    $.get(routes.editApprover, { id: index }, function (res) {
        if (!res || !res.success || !res.data) {
            toastr.error(res.message);
            return;
        }

        const data = res.data;
        $
        $('#approverModalLabel').text("Edit Approver");
        $('#approverID').val(data.Approver_ID).prop('readonly', true);
        $('#IDNo').val(data.ID_No).prop('readonly', true);
        $('#name').val(data.Name).prop('readonly', true);
        $('#emailCC').val(data.Email_CC).prop('readonly', true);
        $('#endorserOnly').prop('checked', data.Endorser_Only === "Y");
        $('#active').prop('checked', data.Active === "Y");

        $('#approverModal').modal('show');
    }).fail(function () {
        toastr.error("An error occurred while trying to load the approver.");
    });
}

function deleteApprover(index, event) {
    if (event) event.preventDefault();
    if (!confirm("Are you sure you want to delete this approver?")) return;

    var token = $('input[name="__RequestVerificationToken"]').val();
    $.post(routes.deleteApprover, { id: index, __RequestVerificationToken: token }, function (res) {
        updateApproverTable(res.data);
        toastr.info(res.message);
    }).fail(function () {
        toastr.error(res.message);
    });
}


function successElement(input, response) {
    if (input === "idNo") {
        $('#IDNo').removeClass('is-invalid').addClass('is-valid').prop('readonly', false);
        $('#name').val(response.data.Name).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
        $('#emailCC').val(response.data.Email_CC).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
    }
    if (input === "name") {
        $('#IDNo').val(response.data.ID_No).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
        $('#name').removeClass('is-invalid').addClass('is-valid').prop('readonly', false);
        $('#emailCC').val(response.data.Email_CC).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
    }
    if (input === "email") {
        $('#IDNo').val(response.data.ID_No).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
        $('#name').val(response.data.Name).removeClass('is-invalid').addClass('is-valid').prop('readonly', true);
        $('#emailCC').removeClass('is-invalid').addClass('is-valid').prop('readonly', false);
    }
}

function failedElement() {
    $('#IDNo').val('').addClass('is-invalid').prop('readonly', false);
    $('#name').val('').addClass('is-invalid').prop('readonly', false);
    $('#emailCC').val('').addClass('is-invalid').prop('readonly', false);
}

function setDefaultElement() {
    $('#IDNo').val('').removeClass('is-invalid is-valid').prop('readonly', false);
    $('#name').val('').removeClass('is-invalid is-valid').prop('readonly', false);
    $('#emailCC').val('').removeClass('is-invalid is-valid').prop('readonly', false);
}

function initToastr() {
    toastr.options = {
        'closeButton': true,
        'debug': false,
        'newestOnTop': false,
        'progressBar': false,
        'positionClass': 'toast-top-right',
        'preventDuplicates': true,
        'showDuration': '1000',
        'hideDuration': '1000',
        'timeOut': '5000',
        'extendedTimeOut': '1000',
        'showEasing': 'swing',
        'hideEasing': 'linear',
        'showMethod': 'fadeIn',
        'hideMethod': 'fadeOut',
        'toastClass': 'custom-toastr'
    }
}

function saveSelectedEmpName() {

    var selectedName;
    var selectedIdNo;
    var selectedEmailCC;

    const modalName = $("#employeeModalLabel").text();
    const isName = modalName.includes("Name");
    const isEmail = modalName.includes("Email");



    if (isName) {
        isEmployeeNameSelected = true;
        selectedName = $('#empName').val();
        selectedIdNo = $('#selIDNo').val();
        selectedEmailCC = $('#selEmailCC').val();
        $("#IDNo").val(selectedIdNo).addClass('is-valid').prop('readonly', true);
        $("#name").val(selectedName).addClass('is-valid').prop('readonly', false);
        $("#emailCC").val(selectedEmailCC).addClass('is-valid').prop('readonly', true);
    }

    if (isEmail) {
        isEmployeeNameSelected = true;
        selectedName = $('#selName').val();
        selectedIdNo = $('#selIDNo').val();
        selectedEmailCC = $('#empEmail').val();
        $("#IDNo").val(selectedIdNo).addClass('is-valid').prop('readonly', true);
        $("#name").val(selectedName).addClass('is-valid').prop('readonly', true);
        $("#emailCC").val(selectedEmailCC).addClass('is-valid').prop('readonly', false);
    }
   

    $('#employeeModal').modal('hide');
}

function isSelectedEmpName() {
    toastr.clear();
    $("#empNameLabel").text("Name")
    if (!isEmployeeNameSelected) {
        toastr.info("Please select employee.");
        setDefaultElement();
    }
}

function onIDNoChange() {

    const selectedIDNo = $('#IDNo').val().trim();

    if ($('#IDNo').prop('readonly')) {
        return;
    }

    if (!selectedIDNo) {
        setDefaultElement()
        return;
    }

    $.get(routes.getEmployeeById, { id: selectedIDNo }, function (response) {
        toastr.clear();
        if (response.success) {
            //toastr.success("Employee found.\n" + response.data.ID_No);
            successElement("idNo", response)
        } else {
            failedElement()
            toastr.warning("No record found.");
        }
    }).fail(function () {
        failedElement()
        toastr.clear();
        toastr.error("An error occurred while searching for the employee.");
    });
}

function onNameChange() {
    toastr.clear();
    const selectedName = $('#name').val().trim();

    if ($('#name').prop('readonly')) {
        return;
    }

    if (selectedName.length == 1) {
        toastr.warning("2 characters minimum.")
        setDefaultElement();
        return;
    }

    if (!selectedName) {
        setDefaultElement();
        return;
    }

    isHiddenFields("name")

    $.get(routes.getEmployeeByName, { name: selectedName }, function (response) {
        if (response && response.success && Array.isArray(response.data) && response.data.length > 0) {
            isEmployeeNameSelected = false;
            
            const nameSelect = $('#empName').empty();
            nameSelect.append(`<option value=""></option>`);

            $.each(response.data, function (i, item) {
                nameSelect.append(`<option value="${item.Name}">${item.Name}</option>`);
            });

            nameSelect.trigger("chosen:updated");
            $('#employeeModal').modal('show');
        } else {
            setDefaultElement()
            toastr.clear();
            toastr.warning("No record found.");
        }
    }).fail(function () {
        setDefaultElement()
        toastr.clear();
        toastr.error("An error occurred while searching for the employee.");
    });
}

function onEmpNameChange() {
    var selectedValue = $('#empName').val();;
    if (!selectedValue) {
        return;
    }

    $.get(routes.getEmployeeByFullName, { name: selectedValue }, function (response) {
            isEmployeeNameSelected = false;
        if (response.success) {
            $('#selIDNo').val(response.data.ID_No);
            $('#selEmailCC').val(response.data.Email);
        } else {
            return
        }
    }).fail(function () {
        toastr.error("An error occurred while searching for the employee.");
    });
}

function onEmailCCChange() {

    toastr.clear();
    const selectedEmail = $('#emailCC').val().trim();

    if ($('#emailCC').prop('readonly')) {
        return;
    }
  
    if (selectedEmail.length == 1) {
        toastr.warning("2 characters minimum.")
        setDefaultElement();
        return;
    }

    if (!selectedEmail) {
        setDefaultElement();
        return;
    }

    isHiddenFields("email")

    $.get(routes.getEmployeeByEmail, { email: selectedEmail }, function (response) {
        if (response && response.success && Array.isArray(response.data) && response.data.length > 0) {
            isEmployeeNameSelected = false;
            
            const nameSelect = $('#empEmail').empty();
            nameSelect.append(`<option value=""></option>`);

            $.each(response.data, function (i, item) {
                nameSelect.append(`<option value="${item.Email}">${item.Email}</option>`);
            });

            nameSelect.trigger("chosen:updated");
            $('#employeeModal').modal('show');
        } else {
            setDefaultElement()
            toastr.clear();
            toastr.warning(response.message);
        }
    }).fail(function () {
        setDefaultElement()
        toastr.clear();
        toastr.error("An error occurred while searching for the employee.");
    });
}

function onSelectedEmailChange() {
    var selectedValue = $('#empEmail').val();

    if (!selectedValue) {
        return;
    }
    $.get(routes.getSelectedEmail, { email: selectedValue }, function (response) {
        isEmployeeNameSelected = false;
        if (response.success) {
            $('#selIDNo').val(response.data.ID_No);
            $('#selName').val(response.data.Name);
        } else {
            toastr.info(response.message);
            return
        }
    }).fail(function () {
        toastr.error("An error occurred while searching for the employee.");
    });
}

function isHiddenFields(name) {

    if (name == "name") {
        $("#employeeModalLabel").text("Select Employee Name")
        $("#empNameLabel").text("Name").css("display", "block")
        $("#empName").css("display", "block")
    } else {
        $("#empNameLabel").text("Name").css("display", "none")
        $("#empName").css("display", "none")
    }

    if (name == "email") {
        $("#employeeModalLabel").text("Select Employee Email")
        $("#empEmailLabel").text("Email").css("display", "block")
        $("#empEmail").css("display", "block")
    } else {
        $("#empEmailLabel").text("Email").css("display", "none")
        $("#empEmail").css("display", "none")
    }
}

// ================================ Modals & Validation ==============================

function openApproverModal(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
     $('#endorserOnly').prop('checked', true);
     $('#active').prop('checked', true);
    loadLastRecordApprover();
}

function closeApproverModal(id, clearFn) {
    clearFn?.();
    $(id).modal('hide');
}

function clearApproverModal() {
    $('#editIndex').val(-1);
    setDefaultElement();
    $('#approverModalLabel').text("Add Approver");

}

function validateApproverForm() {
    let isValid = true, messages = [];

    const idNo = $('#IDNo').val();
    const name = $('#name').val();
    const email = $('#emailCC').val();

    if (!idNo) messages.push("✔ Please enter an ID No.");
    if (!name) messages.push("✔ Please enter a Name.");
    //if (!email) messages.push("✔ Please enter an Email.");

    if (messages.length > 0) {
        toastr.warning(messages.join("<br />"));
        isValid = false;
    }

    return isValid;
}



