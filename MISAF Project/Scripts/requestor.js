// ================================ Config & Routes =================================
const routes = {
    getIndex: '/Requestor/Index',
    createRequestor: '/Requestor/CreateRequestor',
    createRequest: '/Requestor/CreateRequest',
    editMisafRequest: '/Requestor/EditMisafRequest',
    createAttachment: '/Requestor/CreateAttachment',
    editRequest: '/Requestor/EditRequest',
    deleteRequest: '/Requestor/DeleteRequest',
    getApprovers: '/Requestor/GetApprovers',
    getRequestMain: '/Requestor/GetRequestMain',
    getRequestedFor: '/Requestor/GetRequestedFor',
    deleteAttachment: '/Requestor/DeleteAttachment',
    saveMisaf: '/Requestor/SaveMisafAsync',
    updateMisaf: '/Requestor/UpdateMisafAsync',
    sendErrorEmail: '/Requestor/SendErrorEmail',
};

// ================================ Initialize DOM =================================
$(document).ready(function () {
    initToastrRequestor();
    //loadInitialRequestMainTable();
    initSelect2();
    initEventsRequestor();
    document.getElementById("remarksModal")?.removeAttribute("aria-hidden");
    document.getElementById("remarksModal")?.focus();
    document.getElementById("approverModal")?.removeAttribute("aria-hidden");
    document.getElementById("approverModal")?.focus();
    document.getElementById("imageModal")?.removeAttribute("aria-hidden");
    document.getElementById("imageModal")?.focus();

    // Bind save buttons for remarksModal
    $('#btnSaveRemarksEndorse').click(function () {
        SaveEndorse({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: 'Approved'
        });
    });

    $('#btnSaveRemarksReject').click(function () {
        SaveEndorse({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: 'Disapproved'
        });
    });

    $('#addMisaf').click(function () {
        $.get(routes.createRequestor, function (res) {
            if (res.success) {
                $('#requestorModalLabel').text('Create MISAF');
                openModalRequestor('#requestorModal', clearRequestorModal);
                loadSelectedMisaf(res.main, res.users, res.approvers, true);
                loadReaquestDetailTable(res.details);
                loadAttachmentTable(res.attachments);
                updateAttachmentRequestorTable(res.attachments, false);
            } else {
                toastr.info(res.message, 'Info');
            }
        }).fail(function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
        });
    });

    // Request button
    $('#addRequest').click(function (e) {
        e.preventDefault()
        $.get(routes.createRequest, function (res) {
            if (res.success) {
                $('#requestModalLabel').text("Add Request");
                openModalRequestor("#requestModal", clearAddRequest)

                // category select
                const categorySelect = $('#category').empty();
                $.each(res.categories, function (i, group) {
                    const optGroup = $('<optgroup>').attr('label', group.GroupName).attr('value', group.CategoryId);
                    $.each(group.Categories, function (j, category) {
                        optGroup.append($('<option>').val(category).text(category));
                    });
                    categorySelect.append(optGroup);
                });
                categorySelect.trigger("change.select2");

                // reasons select
                const reasonSelect = $('#reason').empty();
                reasonSelect.append(`<option value=""></option>`)
                $.each(res.reasons, function (i, reason) {
                    reasonSelect.append(`<option value="${reason.Reason_ID}">${reason.Reason}</option>`);
                });
                reasonSelect.trigger("change.select2");
                
            } else {
                toastr.info(res.message, 'Info');
            }
        }).fail(function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
        });
    });

    $('#btnSaveRequest').click(function () {
        if (!validateAddRequestForm()) return;

        SaveRequest({
            Index: $('#editIndex').val(),
            CategoryGroupId: $('#category').val(),
            Category: $('#category').val(),
            Reason_ID: $('#reason option:selected').val(),
            Reason: $('#reason option:selected').text(),
            RequestProblemRecommendation: $('#requestProblemRecommendation').val().trim(),
            Status: "For Approval",
        });
    });


    // Attachment button
    $('#addAttachment').click(function (e) {
        e.preventDefault()
        $('#attachmentModalLabel').text("Add Attachment");
        openModalRequestor("#attachmentModal", clearAttachment)
    });

    $('#btnSaveAttachment').click(function () {
        if (!validateAddAttachmentForm()) return;
        saveAttachmentRequestor();
    });


    // Saving MISAF button
    $('#btnSaveMisaf').click(function () {
        if (!validateAddMisafForm({
            FinalApprover: $('#selectFinalApprover option:selected').text(),
            PreApproved: $('#preApproved').is(":checked"),

        })) return;

        SaveMISAF({
            DateRequested: $('#Date_Requested').val(),
            RequestedBy: $('#Requested_By').val(),
            RequestedFor: $('#selectRequestedFor').val(),
            FinalApprover: $('#selectFinalApprover option:selected').text(),
            PreApproved: $('#preApproved').is(":checked"),
            Status: $('#Status').val(),
            EndorsedNotedBy: $('#selectEndorser option:selected').text(),
            TargetDate: $('#MAF_Target_Date').val(),
        });
    });

    // Update MISAF button
    $('#btnUpdateMisaf').click(function () {
        if (!validateAddMisafForm({
            FinalApprover: $('#selectFinalApprover option:selected').text(),
            PreApproved: $('#preApproved').is(":checked"),
        })) return;

        UpdateMISAF({
            MafNo: $('#MAF_No').val(),
            DateRequested: $('#Date_Requested').val(),
            RequestedBy: $('#Requested_By').val(),
            RequestedFor: $('#Requested_For').val(),
            FinalApprover: $('#selectFinalApprover option:selected').text(),
            PreApproved: $('#preApproved').is(":checked"),
            Status: $('#Status').val(),
            EndorsedNotedBy: $('#selectEndorser option:selected').text(),
            TargetDate: $('#MAF_Target_Date').val(),
        });
    });

    // Requested For
    $('#Requested_For').on('input', function () {
        var userInput = $(this).val().trim().toLowerCase();

        // Clear select if input is empty or too short
        if (userInput === "" || userInput.length <= 1) {
            $('#selectRequestedFor').empty().trigger("change.select2");
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: routes.getRequestedFor,
            type: 'POST',
            data: { name: userInput, __RequestVerificationToken: token },
            success: function (res) {
                if (res.success) {
                    const requestedFor = $('#selectRequestedFor').empty();
                    requestedFor.append(`<option value=""></option>`);

                    // Filter and populate options directly from response
                    var cnt = 0;
                    var empID;
                    $.each(res.employee, function (i, employee) {
                        if (employee.Name.toLowerCase().includes(userInput) || employee.ID_No == userInput) {
                            cnt++;
                            empID = employee.ID_No;
                            requestedFor.append(`<option value="${employee.ID_No}">${employee.Name}</option>`);
                        }
                    });

                    // Trigger change event for select2 to update the UI
                    requestedFor.trigger("change.select2");
                    if (cnt == 1) {
                        requestedFor.val(empID).trigger("change.select2");
                        $('#Requested_For').val(empID);
                    }
                } else {
                    $('#selectRequestedFor').empty();
                }
            },
            error: function () {
                toastr.error("An error occurred while fetching employees.", "Error");
            }
        });
    });

    $('#selectRequestedFor').on('change', function () {
        var selectedEmp = $(this).val()
        if (selectedEmp != "" || selectedEmp != null) {
            $('#Requested_For').val(selectedEmp);
        }
    });
    
});

function initEventsRequestor() {
    $('#selectEndorser').on('change', onSelectEndorserChange);
}



// Set up global AJAX pre-loader
function setupGlobalAjaxPreloader() {
    $(document).ajaxStart(function () {
        $("#loading").show();
    }).ajaxStop(function () {
        $("#loading").hide();
    });
}

function initSelect2() {
    $('#selectEndorser, #selectFinalApprover, #category, #reason, #selectRequestedFor').select2({
        theme: 'bootstrap-5',
        width: 'auto', // Responsive width
        placeholder: 'Select an option',
        allowClear: false,
        height: '100%',
        dropdownAutoWidth: false,
        allowClear: true, // Optional: Clear selection
        minimumResultsForSearch: Infinity // Disable search for small lists
    });
}

function onSelectEndorserChange() {
    $.get(routes.getApprovers, function (response) {
        if (response.success) {
            const endorser = $('#selectEndorser').val();
            const approverSelect = $('#selectFinalApprover').empty();
            $.each(response.data, function (i, res) {
                if (endorser != res.Approver_ID && res.Endorser_Only != "Y") {
                    approverSelect.append(`<option value=""></option>`);
                    approverSelect.append(`<option value="${res.Approver_ID}">${res.Name}</option>`);
                }
            });
            approverSelect.trigger("change.select2");
        }
    });
}


function initToastrRequestor() {
    toastr.options = {
        'closeButton': true,
        'debug': false,
        'newestOnTop': false,
        'progressBar': false,
        'preventDuplicates': false,
        'showDuration': '1000',
        'hideDuration': '1000',
        'timeOut': '5000',
        'extendedTimeOut': '1000',
        'showEasing': 'swing',
        'hideEasing': 'linear',
        'showMethod': 'fadeIn',
        'hideMethod': 'fadeOut',
        'toastClass': 'custom-toastr',
        'zIndex': 10000 // Ensure Toastr is above pre-loader
    };
}

function loadInitialRequestMainTable() {
    $.get(routes.getRequestMain, function (res) {
        if (res.success) {
            loadRequestMainTable(res.main);
        } else {
            toastr.error(res.message || 'Failed to load requestor table.', 'Error');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

function loadRequestMainTable(data) {
    const table = $('#misafTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#misafTableBody').empty();
    $.each(data, function (i, item) {
        const statusDate = item.Status_Remarks ? formatDate(item.Status_DateTime) : '';
        var buttons = '';
        if (item.Status_Updated_By.trim() == "" || item.Status_Updated_By == null) {
            buttons += `<button type="button" data-maf-no="${item.MAF_No}" class="btn btn-info btn-sm me-1 btn-edit-misaf" data-bs-toggle="tooltip" data-bs-title="Edit"><i class="mdi mdi-pencil"></i></button>`;
        }
        buttons += `<button type="button" data-maf-no="${item.MAF_No}" class="btn btn-info btn-sm me-1 btn-view-misaf" data-bs-toggle="tooltip" data-bs-title="View"><i class="mdi mdi-eye"></i></button>`;

        let rowClass = '';
        if (item.Status === "Approved" || item.Status === "Done") {
            rowClass = 'table-success';
        } else if (item.Status === "Disapproved" || item.Status === "Reject") {
            rowClass = 'table-danger';
        } else if (item.Status === "On Going" || item.Status === "For Acknowledgement MIS") {
            rowClass = 'table-warning';
        } else if (item.Status === "On Hold") {
            rowClass = 'table-info';
        }

        body.append(`<tr class="${rowClass}">
                <td>${item.MAF_No}</td>
                <td>${item.Requested_For || item.Requestor_Name }</td>
                <td>${item.Requestor_Name}</td>
                <td>${item.Status || ''}</td>
                <td>${statusDate}</td>
                <td>${item.Status_Updated_By || ''}</td>
                <td>${item.Status_Remarks || ''}</td>
                <td>${buttons}</td>
            </tr>`);
    });

    table.DataTable({
        paging: true,
        searching: true,
        ordering: true,
        lengthChange: true,
        lengthMenu: [[10, 50, 100], [10, 50, 100]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();

    $('.btn-edit-misaf').off('click').on('click', function () {
        editMisaf($(this).data('maf-no'), false);
    });

    $('.btn-view-misaf').off('click').on('click', function () {
        editMisaf($(this).data('maf-no'), true);
    });
}

function loadReaquestDetailTable(data) {
    const table = $('#requestTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#requestTableBody').empty();
    $.each(data, function (i, item) {
        let buttons = '';
            buttons += `<button type="button" class="btn btn-info btn-sm me-1 mb-1 btn-edit" data-record-id="${i}" data-bs-toggle="tooltip" data-bs-title="Edit"><i class="mdi mdi-pencil"></i></button>`;
            buttons += `<button type="button" class="btn btn-danger btn-sm me-1 mb-1 btn-delete" data-record-id="${i}" data-bs-toggle="tooltip" data-bs-title="Delete"><i class="mdi mdi-delete"></i></button>`;

        var reason = 'N/A';
        if (item.Reason != null) {
           reason =  item.Reason
        }

        const today = new Date();
        const formattedDate = today.toISOString().split('T')[0];

        let rowClass = '';
        if (item.Status === "Approved" || item.Status === "Done") {
            rowClass = 'table-success';
        } else if (item.Status === "Disapproved" || item.Status === "Reject") {
            rowClass = 'table-danger';
        } else if (item.Status === "On Going" || item.Status === "For Acknowledgement MIS") {
            rowClass = 'table-warning';
        } else if (item.Status === "For Acknowledgement MIS") {
            rowClass = 'table-primary';
        } else if (item.Status === "On Hold") {
            rowClass = 'table-info';
        }

        body.append(`<tr class="${rowClass}">
            <td>${item.Category}</td>
            <td>${item.RequestProblemRecommendation}</td>
            <td>${reason}</td>
            <td>${item.Status}</td>
            <td>${formattedDate}</td>
            <td>${buttons}</td>
        </tr>`);
    });

    table.DataTable({
        bInfo: false,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false,
        lengthMenu: [[50, 100], [50, 100]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();

    // Bind edit/delete buttons
    $('.btn-edit').off('click').on('click', function () {
        edit($(this).data('record-id'));
    });

    $('.btn-delete').off('click').on('click', function () {
        deleteReq($(this).data('record-id'));
    });

   
}

function loadSelectedMisaf(data, users, approvers, isNew) {

    const today = new Date();
    var employees = [
        { ID: data.Requestor_ID_No, Name: data.Requestor_Name },
    ];

    var endoserID = null;
    var approverID = null;
    const approverSelect = $('#selectFinalApprover').empty();
    const endorserSelect = $('#selectEndorser').empty();

    $.each(approvers, function (i, item) {

        if (item.Endorser_Only == "Y") {
            endorserSelect.append(`<option value=""></option>`);
            endorserSelect.append(`<option value="${item.Approver_ID}">${item.Name}</option>`);
            if (item.Name == data.Endorsed_By) {
                endoserID = item.Approver_ID;
            }
        }

        if (item.Endorser_Only != "Y") {
            approverSelect.append(`<option value=""></option>`);
            approverSelect.append(`<option value="${item.Approver_ID}">${item.Name}</option>`);
            if (item.Name == data.Final_Approver) {
                approverID = item.Approver_ID;
            }
        }

    });


    if (isNew) {
        const formattedDate = today.toISOString().split('T')[0];
        $('#editMafNo').hide();
        $('#Status').val("For Approval");
        $('#mafNoAcknowledge').val(data.MAF_No);
        $('#Requested_By').val(users.UserLogin);
        $('#Date_Requested').val(formattedDate);
        //$('#preApproved').prop('checked', true).prop('disabled', false);
        $('#preApproved').prop('disabled', false);
        $('#Requested_For').prop('disabled', false);
        $('#addRequest').prop('disabled', false);
        $('#addAttachment').prop('disabled', false);
        $('#selectEndorser').prop('disabled', false);
        $('#selectFinalApprover').prop('disabled', false);
        $('#selectRequestedFor').prop('disabled', false);
        $('#MAF_Target_Date').prop('disabled', false);

        HiddenFields(false);
    } else {
        $('#editMafNo').show();
        $('#MAF_No').val(data.MAF_No);
        const formattedDate = formatDotNetDate(data.DateTime_Requested);
        const dateEndorse = formatDotNetDate(data.DateTime_Endorsed);
        const dateFinalApprover = formatDotNetDate(data.DateTime_Approved);
        const dateStatus = formatDotNetDate(data.Status_DateTime);
        $('#Date_Requested').val(formattedDate);
        $('#DateTime_Endorsed').val(endoserID != null ? dateEndorse : null);
        $('#Endorser_Remarks').val(endoserID != null ? data.Endorser_Remarks : null);
        $('#DateTime_Approved').val(approverID != null ? dateFinalApprover : null);
        $('#Final_Approver_Remarks').val(data.Final_Approver_Remarks || null);
        $('#Status').val(data.Status || null);
        $('#Status_DateTime').val(dateStatus || null);
        $('#MAF_Target_Date').val(data.Target_Date);

        if (data.PreApproved === 'Y') {
            $('#preApproved').prop('checked', true).prop('disabled', false);
        } else {
            $('#preApproved').prop('checked', false).prop('disabled', false);
        }
        HiddenFields(true);

        //var filteredEmployees = employees.filter(function (employee) {
        //    return employee.Requestor_ID_No.toLowerCase().includes(data.Requestor_Name);
        //});

        if (data.Requested_By != null) {
            const requestedFor = $('#selectRequestedFor').empty();
            requestedFor.append(`<option value=""></option>`);
            $.each(employees, function (i, employee) {
                requestedFor.append(`<option selected value="${employee.ID}">${employee.Name}</option>`);
            });
            requestedFor.trigger("change.select2");
            $('#Requested_For').val(data.Requested_By != null ? data.Requestor_ID_No : null)
        }
        
        $('#Requested_By').val(data.Requested_By || data.Requestor_Name)
    }

    if (!isNew) {
        approverSelect.val(approverID).trigger("change.select2");
        endorserSelect.val(endoserID).trigger("change.select2");
        $('#requestorModalLabel').text("Edit MISAF");
        $('#btnUpdateMisaf').show();
        $('#btnSaveMisaf').hide();
    } else {
        approverSelect.trigger("change.select2");
        endorserSelect.trigger("change.select2");
        $('#btnUpdateMisaf').hide();
        $('#btnSaveMisaf').show();
    }
}

function loadAttachmentTable(data) {
    const table = $('#attachmentTable');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBody').empty();
    $.each(data, function (i, item) {
        const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
        const ext = item.Filename.split('.').pop().toLowerCase();
        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
        const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageModal('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.Filename}</a>`;

        body.append(`
            <tr>
                <td>${item.Filename}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="viewAttachmentRequestor(${i}, event)" class="btn btn-info btn-sm" data-bs-toggle="tooltip" data-bs-title="View"><i class="mdi mdi-eye"></i></button>
                </td>
            </tr>`);
    });

    table.DataTable({
        bInfo: false,
        paging: false,
        searching: false,
        ordering: false,
        lengthChange: false,
        lengthMenu: [[1, 5], [1, 5]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();
}

function showImageModal(fileUrl) {
    const modal = $('#imageModal');
    const modalImage = modal.find('#modalImage');
    modalImage.attr('src', fileUrl);
    modal.modal('show');
}

function viewAttachmentRequestor(index, event) {
    event.preventDefault();
    const item = attachmentData[index];
    const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
    const ext = item.Filename.split('.').pop().toLowerCase();
    const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
    const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

    if (isImage) {
        showImageModal(fileUrl);
    } else {
        window.open(fileUrl, '_blank');
    }
}


// ================================ Edit Misaf =================================
function editMisaf(index, isView) {
    $.post(routes.editMisafRequest, { index }, function (res) {
        if (res.success) {
            $('#requestorModalLabel').text('Requestor Management');
            openModalRequestor('#requestorModal', clearRequestorModal);
            loadSelectedMisaf(res.main, res.users, res.approvers, false);
            loadReaquestDetailTable(res.details);
            //loadAttachmentTable(res.attachments);
            updateAttachmentRequestorTable(res.attachments, res.isEdit);

            // category select
            const categorySelect = $('#category').empty();
            $.each(res.categories, function (i, group) {
                const optGroup = $('<optgroup>').attr('label', group.GroupName).attr('value', group.CategoryId);
                $.each(group.Categories, function (j, category) {
                    optGroup.append($('<option>').val(category).text(category));
                });
                categorySelect.append(optGroup);
            });
            categorySelect.val(res.main.Category).trigger("change.select2");

            // reasons select
            const reasonSelect = $('#reason').empty();
            reasonSelect.append(`<option value=""></option>`)
            $.each(res.reasons, function (i, reason) {
                reasonSelect.append(`<option value="${reason.Reason_ID}">${reason.Reason}</option>`);
            });
            reasonSelect.trigger("change.select2");

            if (isView) {
                var data = res.main
                var Status_DateTime = null;
 
                if (data.Status_DateTime != null) {
                    Status_DateTime = formatDotNetDate(data.Status_DateTime);
                }

                $('#addRequest').prop('disabled', isView);
                $('#Requested_For').prop('disabled', isView);
                $('#selectEndorser').prop('disabled', isView);
                $('#selectFinalApprover').prop('disabled', isView);
                $('#selectRequestedFor').prop('disabled', isView);
                $('.btn-edit').prop('disabled', isView);
                $('.btn-delete').prop('disabled', isView);
                $('.btn-attachment-edit').prop('disabled', isView);
                $('.btn-attachment-delete').prop('disabled', isView);
                $('#preApproved').prop('disabled', isView);
                $('#addAttachment').prop('disabled', isView);
                $('#btnUpdateMisaf').prop('disabled', isView);
                $('#requestorModalLabel').text("View MISAF");
                $('#DateTime_Endorsed').prop('disabled', isView);
                $('#Endorser_Remarks').prop('disabled', isView);
                $('#DateTime_Approved').prop('disabled', isView);
                $('#Final_Approver_Remarks').prop('disabled', isView);
                $('#Status_DateTime').val(Status_DateTime).prop('disabled', isView);
                $('#Status').prop('disabled', isView);
                HiddenFields(true);
            } else {
                $('#addRequest').prop('disabled', isView);
                $('#Requested_For').prop('disabled', isView);
                $('#selectEndorser').prop('disabled', isView);
                $('#selectFinalApprover').prop('disabled', isView);
                $('#selectRequestedFor').prop('disabled', isView);
                $('.btn-edit').prop('disabled', isView);
                $('.btn-delete').prop('disabled', isView);
                $('.btn-attachment-edit').prop('disabled', isView);
                $('.btn-attachment-delete').prop('disabled', isView);
                $('#preApproved').prop('disabled', isView);
                $('#addAttachment').prop('disabled', isView);
                $('#btnUpdateMisaf').prop('disabled', isView);
                HiddenFields(false);
            }

        } else {
            toastr.info(res.message, 'Info');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

// ================================ Add Request CRUD =================================
function SaveRequest(originalData) {

    const token = $('input[name="__RequestVerificationToken"]').val();
    const data = {
        Index: originalData.Index,
        Category: originalData.Category,
        ReasonPurpose: originalData.Reason,
        Reason: originalData.Reason,
        Reason_ID: originalData.Reason_ID,
        RequestProblemRecommendation: originalData.RequestProblemRecommendation,
        Status: originalData.Status,
    };

    $("#loading").show();

    $.ajax({
        url: routes.createRequest,
        type: 'POST',
        data: { data, __RequestVerificationToken: token },
        timeout: 30000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveEndorse failed:', res);
                return;
            }

            setTimeout(() => {
                try {
                    loadReaquestDetailTable(res.details);
                    toastr.success(res.message, 'Success', { timeOut: 5000 });
                    $('#requestModal').modal('hide');
                } catch (error) {
                    console.error('Error in setTimeout function:', error);
                    toastr.error('Failed to process data.', 'Error', { timeOut: 5000 });
                }
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error.errors || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveRequest AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
        }
    });
}

function edit(index) {

    $.post(routes.editRequest, { index }, function (data) {
        if (!data) {
            toastr.error("Failed to load request details.");
            return;
        }

        $('#category').val(data.Category).trigger("change.select2");
        $('#reason').val(data.Reason_ID).trigger("change.select2")

        $('#requestProblemRecommendation').val(data.RequestProblemRecommendation);
        $('#editIndex').val(index);
        $('#requestModalLabel').text("Edit Request");
        $('#requestModal').modal('show');
    }).fail(function () {
        toastr.error("An error occurred while trying to load the request.");
    });
}

function deleteReq(index) {
    if (!confirm("Are you sure you want to delete this request?")) return;

    $.post(routes.deleteRequest, { index }, function (res) {
        loadReaquestDetailTable(res.details);
        toastr.info("Deleted", "Info");
    }).fail(function () {
        toastr.error("Failed to delete the request. Please try again.");
    });
}

// ================================ Add Attachment CRUD =================================
function saveAttachmentRequestor() {
    const formData = new FormData();
    formData.append('file', $('#attachment')[0].files[0]);
    formData.append('index', $('#editIndex').val());

    $("#loading").show();
    $.ajax({
        url: routes.createAttachment,
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                toastr.success("Added", "Success")
                updateAttachmentRequestorTable(res.attachments, false);
                $('#attachmentModal').modal('hide');
                $('#editIndex').val(-1);
            } else toastr.warning(res.message || 'Upload failed.', "Warning");
        },
        error: function (xhr, status, error) {
            toastr.error(`Attachment failed: ${error.errors || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveAttachment AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
        }
    });
}

function updateAttachmentRequestorTable(data, isEdit) {
    const table = $('#attachmentTable');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBody').empty();
    $.each(data, function (i, item) {
        var ext = '';
        var fileName = '';

        if (isEdit != null && isEdit) {
            ext = item.FileName.split('.').pop().toLowerCase();
            fileName = item.FileName;
        } else {
            ext = item.FileName.split('.').pop().toLowerCase();
            fileName = item.FileName;
        }

        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);

        const fileUrl = `/App_Data/TempUploads/${fileName}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageRequestorModal('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.OriginalName}</a>`;

        body.append(`
            <tr>
                <td>${item.OriginalName}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="editAttachment(${i}, event)" class="btn btn-info btn-sm btn-attachment-edit"><i class="mdi mdi-pencil"></i></button>
                    <button onclick="deleteAttachment(${i}, event)" class="btn btn-danger btn-sm btn-attachment-delete"><i class="mdi mdi-delete"></i></button>
                </td>
            </tr>`);
    });
    table.DataTable({ paging: true, searching: true, ordering: true, lengthChange: false });
}

function editAttachment(index, event) {
    if (event) event.preventDefault();
    const item = attachmentData[index];
    const ext = item.FileName.split('.').pop().toLowerCase();
    const fileUrl = `/App_Data/TempUploads/${item.FileName}`;
    $('#attachmentPreview').show();
    $('#editIndex').val(index);
    $('#attachment').val(null);
    $('#attachmentModalLabel').text("Edit Attachment");
    $('#attachmentPreview').html(
        ['jpg', 'jpeg', 'png', 'gif'].includes(ext)
            ? `<img src="${fileUrl}" class="img-fluid" style="max-height: 300px;">`
            : `<iframe src="${fileUrl}" width="100%" height="400px" frameborder="0"></iframe>`
    );
    $('#attachmentModal').modal('show');
}

function deleteAttachment(index, event) {
    if (event) event.preventDefault(); // Prevent default action if triggered by a link or form
    if (!confirm("Are you sure you want to delete this attachment?")) return;

    $.post(routes.deleteAttachment, { index }, function (res) {
        if (res.success) {
            toastr.info("Attachment deleted successfully.");
            updateAttachmentRequestorTable(res.attachments, false);
        } else {
            toastr.warning(res.message || "Deletion failed.");
        }
    }).fail(function () {
        toastr.error("An error occurred while deleting the attachment.");
    });
}


function showImageRequestorModal(imageUrl) {
    $('#imageModal .modal-body').html(`<img src="${imageUrl}" class="img-fluid" style="max-height: 500px;">`);
    $('#imageModal').modal('show');
}

// ================================ Saving And Updating MISAF =================================
function SaveMISAF(data) {
    const misaf = {
        DateRequested: data.DateRequested,
        RequestedBy: data.RequestedBy,
        RequestedFor: data.RequestedFor,
        Endorser: data.Endorser,
        FinalApprover: data.FinalApprover,
        StatusDate: data.StatusDate,
        PreApproved: data.PreApproved,
        Status: data.Status,
        EndorsedNotedBy: data.EndorsedNotedBy,
        TargetDate: data.TargetDate
    };

    var token = $('input[name="__RequestVerificationToken"]').val();
    $("#loading").show();
    $.ajax({
        url: routes.saveMisaf,
        type: 'POST',
        data: { data: misaf, __RequestVerificationToken: token },
        success: function (res) {
            if (res.success) {
                toastr.success("Misaf saved successfully.");
                setTimeout(function () {
                    $('#requestorModal').modal('hide');
                    loadRequestMainTable(res.main);
                }, 2000);
            } else {
                let errors = res.errors || [res.message || "Saving failed."];
                let errorHtml = '<ul>';
                errors.forEach(err => {
                    errorHtml += `<li>${err}</li>`;
                });
                errorHtml += `<li>Controller: ${res.controller}</li>`;
                errorHtml += `<li>Action: ${res.action}</li>`;
                errorHtml += '</ul>';
                $('#validationErrors').html(errorHtml);
                $('#validationModal').modal('show');

                // Include controller, action, and input data in additional details
                let additionalDetails = `Input Data: ${JSON.stringify(data)}`;
                if (res.controller && res.action) {
                    additionalDetails += `\nController: ${res.controller}\nAction: ${res.action}`;
                }
                if (res.stackTrace) {
                    additionalDetails += `\nStack Trace: ${res.stackTrace}`;
                }

                $('#sendEmailButton').off('click').on('click', function () {
                    $("#loading").show();
                    $.ajax({
                        url: routes.sendErrorEmail,
                        type: 'POST',
                        data: {
                            errorMessage: errors.join('; '),
                            additionalDetails: additionalDetails,
                            __RequestVerificationToken: token
                        },
                        success: function (response) {
                            if (response.success) {
                                toastr.success("Error email sent successfully to the MIS team.");
                                $('#validationModal').modal('hide');
                            } else {
                                toastr.error(response.message || "Failed to send error email.");
                            }
                        },
                        error: function (xhr) {
                            toastr.error("An error occurred while sending the email: " + xhr.statusText);
                        },
                        complete: function () {
                            $("#loading").hide();
                        }
                    });
                });
            }
        },
        error: function (xhr) {
            let errorHtml = `<p>An unexpected error occurred: ${xhr.statusText}</p>`;
            $('#validationErrors').html(errorHtml);
            $('#validationModal').modal('show');

            // For network failures, stack trace isn't available
            let additionalDetails = `Input Data: ${JSON.stringify(data)}\nStack Trace: Not available (network failure: ${xhr.statusText})`;

            $('#sendEmailButton').off('click').on('click', function () {
                $.ajax({
                    url: routes.sendErrorEmail,
                    type: 'POST',
                    data: {
                        errorMessage: xhr.statusText,
                        additionalDetails: additionalDetails,
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success("Error email sent successfully to the MIS team.");
                            $('#validationModal').modal('hide');
                        } else {
                            toastr.error(response.message || "Failed to send error email.");
                        }
                    },
                    error: function (xhr) {
                        toastr.error("An error occurred while sending the email: " + xhr.statusText);
                    }
                });
            });
        },
        complete: function () {
            $("#loading").hide();
            drawTable(); // TODO: HACKY WAY TO UPDATE, SUBJECT TO CHANGE
        }
    });
}

function UpdateMISAF(data) {
    const misaf = {
        MafNo: data.MafNo,
        DateRequested: data.DateRequested,
        RequestedBy: data.RequestedBy,
        RequestedFor: data.RequestedFor,
        Endorser: data.Endorser,
        FinalApprover: data.FinalApprover,
        StatusDate: data.StatusDate,
        PreApproved: data.PreApproved,
        Status: data.Status,
        EndorsedNotedBy: data.EndorsedNotedBy,
        TargetDate: data.TargetDate,
    };

    var token = $('input[name="__RequestVerificationToken"]').val();
    $("#loading").show();
    $.ajax({
        url: routes.updateMisaf,
        type: 'POST',
        data: { data: misaf, __RequestVerificationToken: token },
        success: function (res) {
            if (res.success) {
                toastr.success("Misaf updated successfully.");
                // Add a 2-second delay before redirecting
                setTimeout(function () {
                    $('#requestorModal').modal('hide');
                    loadRequestMainTable(res.main);

                }, 2000);
            } else {
                let errors = res.errors || [res.message || "Saving failed."];
                let errorHtml = '<ul>';
                errors.forEach(err => {
                    errorHtml += `<li>${err}</li>`;
                });
                errorHtml += '</ul>';
                $('#validationErrors').html(errorHtml);
                $('#validationModal').modal('show');

                // Include stack trace if available
                let additionalDetails = `Input Data: ${JSON.stringify(data)}`;
                if (res.stackTrace) {
                    additionalDetails += `\nStack Trace: ${res.stackTrace}`;
                }

                $('#sendEmailButton').off('click').on('click', function () {
                    $.ajax({
                        url: routes.sendErrorEmail,
                        type: 'POST',
                        data: {
                            errorMessage: errors.join('; '),
                            additionalDetails: additionalDetails,
                            __RequestVerificationToken: token
                        },
                        success: function (response) {
                            if (response.success) {
                                toastr.success("Error email sent successfully to the MIS team.");
                                $('#validationModal').modal('hide');
                            } else {
                                toastr.error(response.message || "Failed to send error email.");
                            }
                        },
                        error: function (xhr) {
                            toastr.error("An error occurred while sending the email: " + xhr.statusText);
                        }
                    });
                });
            }
        },
        error: function (xhr) {
            let errorHtml = `<p>An unexpected error occurred: ${xhr.statusText}</p>`;
            $('#validationErrors').html(errorHtml);
            $('#validationModal').modal('show');

            // For network failures, stack trace isn't available
            let additionalDetails = `Input Data: ${JSON.stringify(data)}\nStack Trace: Not available (network failure: ${xhr.statusText})`;

            $('#sendEmailButton').off('click').on('click', function () {
                $.ajax({
                    url: routes.sendErrorEmail,
                    type: 'POST',
                    data: {
                        errorMessage: xhr.statusText,
                        additionalDetails: additionalDetails,
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success("Error email sent successfully to the MIS team.");
                            $('#validationModal').modal('hide');
                        } else {
                            toastr.error(response.message || "Failed to send error email.");
                        }
                    },
                    error: function (xhr) {
                        toastr.error("An error occurred while sending the email: " + xhr.statusText);
                    }
                });
            });
        },
        complete: function () {
            $("#loading").hide();
            drawTable(); // TODO: HACKY WAY TO UPDATE, SUBJECT TO CHANGE
        }
    });
}
// ================================ Modals & Validation=================================
function clearRequestorModal() {
    $('#requestTableBody').empty();
    $('#attachmentTableBody').empty();
    $('#selectRequestedFor').empty();
    $('#Date_Requested, #MAF_No, #Requested_By, #Requested_For, #Endorsed_By, #DateTime_Endorsed, #Endorser_Remarks, #Final_Approver, #DateTime_Approved, #Final_Approver_Remarks, #Status').val('');
    $('#preApproved').prop('checked', false);
}

function clearAddRequest() {
    $('#editIndex').val(-1);
    $('#category').val('').trigger("change.select2");
    $('#reason').val([]).trigger("change.select2");
    $('#requestProblemRecommendation').val('');
}

function clearAttachment() {
    $('#attachmentPreview').hide();
    $('#editIndex').val(-1);
    $('#attachment').val('');
}

function openModalRequestor(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
}

function validateAddRequestForm() {
    let isValid = true, messages = [];

    const category = $('#category').val();
    const reasons = $('#reason').val();
    const reqProbRecom = $('#requestProblemRecommendation').val().trim();

    if (!category) messages.push("✔ Please select a category.");
    if (!reasons || reasons.length === 0) messages.push("✔ Please select at least one reason.");
    if (!reqProbRecom) messages.push("✔ Please enter a request/problem recommendation.");

    if (messages.length > 0) {
        toastr.warning(messages.join("<br />"));
        isValid = false;
    }

    return isValid;
}

function validateAddAttachmentForm() {
    const file = $('#attachment')[0].files[0];
    if (!file) return toastr.warning("✔ Please select an Attachment."), false;
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
    const maxSize = 5 * 1024 * 1024;
    if (!allowedTypes.includes(file.type)) return toastr.warning("✔ Only JPG, PNG or PDFs are allowed."), false;
    if (file.size > maxSize) return toastr.warning("✔ File size must not exceed 5MB."), false;
    return true;
}

function validateAddMisafForm(data) {
    let isValid = true, messages = [];

    const tableReq = $('#requestTable');
    if ($.fn.DataTable.isDataTable(tableReq)) {
        const rowCount = tableReq.DataTable().rows().count();
        if (rowCount == 0) messages.push('✔ Please add at least one request.');
    }

    if (!data.FinalApprover) messages.push("✔ Final Approver is required.");

    if (data.PreApproved) {
        const tableAttach = $('#attachmentTable');
        if ($.fn.DataTable.isDataTable(tableAttach)) {
            const rowCount2 = tableAttach.DataTable().rows().count();
            if (rowCount2 == 0) messages.push('✔ Please add at least one attachment.');
        }
    }

    if (messages.length > 0) {
        toastr.warning(messages.join("<br />"));
        isValid = false;
    }

    if (!isValid) {
        let errorHtml = '<ul>';
        errors.forEach(err => {
            errorHtml += `<li>${err}</li>`;
        });
        errorHtml += '</ul>';
        $('#validationErrors').html(errorHtml);
        $('#validationModal').modal('show');
    }

    return isValid;
}

function HiddenFields(isView) {
    if (!isView) {
        $('#frm_DateTime_Endorsed').hide();
        $('#frm_Endorser_Remarks').hide();
        $('#frm_DateTime_Approved').hide();
        $('#frm_Final_Approver_Remarks').hide();
    } else {
        $('#frm_DateTime_Endorsed').show();
        $('#frm_Endorser_Remarks').show();
        $('#frm_DateTime_Approved').show();
        $('#frm_Final_Approver_Remarks').show();
    }
}