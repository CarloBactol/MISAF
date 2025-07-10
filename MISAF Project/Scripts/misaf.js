// ================================ Config & Routes =================================
const routes = {
    getRequests: '/MISAF/GetSavedRequest',
    getIndex: '/MISAF/Index',
    sendErrorEmail: '/MISAF/SendErrorEmail',
    getApprovers: '/MISAF/GetApprovers',
    getAttachments: '/MISAF/GetAttachments',
    getGroupedCategories: '/MISAF/GetGroupedCategories',
    getReasonsByCategory: '/MISAF/GetReasonsByCategory',
    getRequestForEdit: '/MISAF/GetRequestForEdit',
    deleteRequest: '/MISAF/DeleteRequest',
    addRequest: '/MISAF/AddRequest',
    addAttachment: '/MISAF/AddAttachment',
    deleteAttachment: '/MISAF/DeleteAttachment',
    saveMisaf: '/MISAF/SaveMisafAsync'
};

let attachmentData = []; // holds current attachment data

// ================================ Initialization ===================================
$(document).ready(function () {
    initSelect2();
    initEvents();
    loadGroupedCategories();
    loadInitialTables();
    initToastr();
    loadApprovers();
});


function initSelect2() {
    $('#endorser, #approver, #category, #reason').select2({
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

function initEvents() {
    $('#category').on('change', onCategoryChange);
    $('#endorser').on('change', onEndorserChange);
    $('#btnAddRequest').on('click', () => openModal('#requestModal', clearRequestModal));
    $('#btnAddAttachment').on('click', () => openModal('#attachmentModal', clearAttachmentModal));
    $('#btnClose, #btnCloseAttachment, #btnImageClose').on('click', () => $('.modal').modal('hide'));
    //$('#uploadForm').submit(function (e) {
    //    e.preventDefault();
    //    saveAttachment();
    //});
}

function loadInitialTables() {
    $.get(routes.getRequests, updateRequestTable);
    $.get(routes.getAttachments, updateAttachmentTable);
}




function initToastr() {
    toastr.options = {
        'closeButton': true,
        'debug': false,
        'newestOnTop': false,
        'progressBar': false,
        'positionClass': 'toast-top-right',
        'preventDuplicates': false,
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


// ================================ Category & Reasons ==============================
function onCategoryChange() {
    $.get(routes.getReasonsByCategory, function (reasons) {
        const reasonSelect = $('#reason').empty();
        $.each(reasons, function (i, reason) {
            reasonSelect.append(`<option value="${reason.Reason}">${reason.Reason}</option>`);
        });
        reasonSelect.trigger("change");
    });
}

function loadGroupedCategories() {
    $.get(routes.getGroupedCategories, function (groups) {
        const categorySelect = $('#category').empty();
        $.each(groups, function (i, group) {
            const optGroup = $('<optgroup>').attr('label', group.GroupName).attr('value', group.CategoryId);
            $.each(group.Categories, function (j, category) {
                optGroup.append($('<option>').val(category).text(category));
            });
            categorySelect.append(optGroup);
        });
        categorySelect.trigger("change.select2");
    });
}

// ================================ Request Management ==============================
function updateRequestTable(data) {
    const table = $('#requestTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#requestTableBody').empty();
    $.each(data, function (i, item) {
        body.append(`
            <tr>
                <td>${item.Category}</td>
                <td>${item.RequestProblemRecommendation}</td>
                <td>${item.ReasonPurpose.join(', ')}</td>
                <td></td>
                <td></td>
                <td>
                    <button onclick="editRequest(${i}, event)" class="btn btn-info btn-sm"><i class="mdi mdi-pencil"></i></button>
                    <button onclick="deleteRequest(${i}, event)" class="btn btn-danger btn-sm"><i class="mdi mdi-delete"></i></button>
                </td>
            </tr>`);
    });
    table.DataTable({ paging: true, searching: true, ordering: true, lengthChange: true, lengthMenu: [[5, 10], [5, 10]] });
}

function saveRequest() {
    if (!validateRequestForm()) return;

    const data = {
        Category: $('#category option:selected').text(),
        RequestProblemRecommendation: $('#reqProbRecom').val(),
        ReasonPurpose: $('#reason').val(),
        Remarks: $('#remarks').val(),
        Index: $('#editIndex').val(),
    };

    const token = $('input[name="__RequestVerificationToken"]').val();
    $.post(routes.addRequest, { data, __RequestVerificationToken: token }, function (data) {
        toastr.success("Added successfuly.")
        updateRequestTable(data);
        $('#requestModal').modal('hide');
        $('#editIndex').val(-1);
    });
}

function editRequest(index, event) {
    if (event) event.preventDefault(); 

    $.post(routes.getRequestForEdit, { index }, function (data) {
        if (!data) {
            toastr.error("Failed to load request details.");
            return;
        }

        $('#category').val(data.Category).trigger("change.select2");
        setTimeout(() => $('#reason').val(data.ReasonPurpose).trigger("change"), 300);

        $('#reqProbRecom').val(data.RequestProblemRecommendation);
        $('#remarks').val(data.Remarks);
        $('#editIndex').val(index);
        $('#requestModalLabel').text("Edit Request");
        $('#requestModal').modal('show');
    }).fail(function () {
        toastr.error("An error occurred while trying to load the request.");
    });
}

function deleteRequest(index, event) {
    if (event) event.preventDefault(); 
    if (!confirm("Are you sure you want to delete this request?")) return;

    $.post(routes.deleteRequest, { index }, function (data) {
        updateRequestTable(data);
        toastr.info("Request deleted successfully.");
    }).fail(function () {
        toastr.error("Failed to delete the request. Please try again.");
    });
}


// ================================ Approvers Details Management ===========================
function loadApprovers() {
    $.get(routes.getApprovers, function (response) {
        const approverSelect = $('#approver').empty();
        const endorserSelect = $('#endorser').empty();

        if (response.success) {
            $.each(response.data, function (i, res) {

                // check if only endorser 

                if (res.Endorser_Only == "Y") {
                    endorserSelect.append(`<option value=""></option>`);
                    endorserSelect.append(`<option value="${res.Approver_ID}">${res.Name}</option>`);
                } 
            
                if (res.Endorser_Only != "Y") {
                    approverSelect.append(`<option value=""></option>`);
                    approverSelect.append(`<option value="${res.Approver_ID}">${res.Name}</option>`);
                } 
               
            });
            approverSelect.trigger("change");
            endorserSelect.trigger("change");
        } else {
            toastr.warning(response.message);
        }
    });
}

function onEndorserChange() {
    $.get(routes.getApprovers, function (response) {
        if (response.success) {
            const endorser = $('#endorser').val();
            const approverSelect = $('#approver').empty();
            $.each(response.data, function (i, res) {
                if (endorser != res.Approver_ID && res.Endorser_Only != "Y") {
                    approverSelect.append(`<option value=""></option>`);
                    approverSelect.append(`<option value="${res.Approver_ID}">${res.Name}</option>`);
                }
            });
            approverSelect.trigger("change");
        }
    });
}

// ================================ Attachment Management ===========================
function updateAttachmentTable(data) {
    const table = $('#attachmentTable');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBody').empty();
    $.each(data, function (i, item) {
        const ext = item.FileName.split('.').pop().toLowerCase();
        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
        const fileUrl = `/App_Data/TempUploads/${item.FileName}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageModal('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.OriginalName}</a>`;

        body.append(`
            <tr>
                <td>${item.OriginalName}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="editAttachment(${i}, event)" class="btn btn-info btn-sm"><i class="mdi mdi-pencil"></i></button>
                    <button onclick="deleteAttachment(${i}, event)" class="btn btn-danger btn-sm"><i class="mdi mdi-delete"></i></button>
                </td>
            </tr>`);
    });
    table.DataTable({ paging: true, searching: true, ordering: true, lengthChange: false });
}

function saveAttachment() {
    if (!validateAttachmentForm()) return;
    const formData = new FormData();
    formData.append('file', $('#attachment')[0].files[0]);
    formData.append('index', $('#editIndex').val());

    $.ajax({
        url: routes.addAttachment,
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                toastr.success("Attachment saved successfully.")
                updateAttachmentTable(res.attachments);
                $('#attachmentModal').modal('hide');
                $('#editIndex').val(-1);
            } else toastr.error(res.message || 'Upload failed.');
        },
        error: function (xhr) {
            toastr.error("Upload error: " + xhr.responseText);
        }
    });
}

function editAttachment(index, event) {
    if (event) event.preventDefault();
    const item = attachmentData[index];
    const ext = item.FileName.split('.').pop().toLowerCase();
    const fileUrl = `/TempUploads/${item.FileName}`;

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
            updateAttachmentTable(res.attachments);
        } else {
            toastr.warning(res.message || "Deletion failed.");
        }
    }).fail(function () {
        toastr.error("An error occurred while deleting the attachment.");
    });
}

// ================================ Saving MISAF Management ===========================



function SaveMisaf(event) {
    if (event) event.preventDefault();

    const data = {
        DateRequested: $('#DateRequested').val(),
        RequestedBy: $('#RequestedBy').val(),
        RequestedFor: $('#RequestedFor').val(),
        Endorser: $('#endorser').val(),
        FinalApprover: $('#approver option:selected').text(),
        StatusDate: $('#StatusDate').val(),
        PreApproved: $('#preApproved').is(":checked"),
        SendEmail: $('#sendEmail').is(":checked"),
        Status: $('#statuses option:selected').text(),
        EndorsedNotedBy: $('#endorser option:selected').text(),
    };

    if (!validateMisafForm(data)) return;

    const token = $('input[name="__RequestVerificationToken"]').val();
    $.post(routes.saveMisaf, { data: data, __RequestVerificationToken: token }, function (res) {
        if (res.success) {
            toastr.success("Misaf saved successfully.");
            // Add a 2-second delay before redirecting
            setTimeout(function () {
                window.location.href = routes.getIndex;
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
    }).fail(function (xhr) {
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
    });
}




// ================================ Modals & Validation ==============================
function openModal(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
}

function clearRequestModal() {
    $('#editIndex').val(-1);
    $('#category').val('').trigger("change");
    $('#reason').val([]).trigger("change");
    $('#reqProbRecom').val('');
    $('#remarks').val('');
    $('#requestModalLabel').text("Add Request");
}

function clearAttachmentModal() {
    $('#editIndex').val(-1);
    $('#attachment').val('');
    $('#attachmentPreview').empty();
    $('#attachmentModalLabel').text("Add Attachment");
}

function validateMisafForm(data) {
    let isValid = true, messages = [];

    const tableReq = $('#requestTable');
    if ($.fn.DataTable.isDataTable(tableReq)) {
        const rowCount = tableReq.DataTable().rows().count();
        if (rowCount == 0) messages.push('✔ Please add at least one request.');
    }

    if (!data.FinalApprover) messages.push("✔ Final Approver is required.");

    if (data.PreApproved) {
        const tableAttach = $('#requestTable');
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

function validateRequestForm() {
    let isValid = true, messages = [];

    const category = $('#category').val();
    const reasons = $('#reason').val();
    const reqProbRecom = $('#reqProbRecom').val().trim();
    //const targetDateStr = $('#targetDate').val();

    if (!category) messages.push("✔ Please select a category.");
    if (!reasons || reasons.length === 0) messages.push("✔ Please select at least one reason.");
    if (!reqProbRecom) messages.push("✔ Please enter a request/problem recommendation.");
    //if (!targetDateStr) {
    //    messages.push("✔ Please select a target date.");
    //} else {
    //    const selectedDate = new Date(targetDateStr);
    //    const today = new Date();
    //    // Reset time part of both dates
    //    selectedDate.setHours(0, 0, 0, 0);
    //    today.setHours(0, 0, 0, 0);

    //    if (selectedDate < today) {
    //        messages.push("✔ Target date must not be earlier than today.");
    //    }
    //}

    if (messages.length > 0) {
        toastr.warning(messages.join("<br />"));
        isValid = false;
    }

    return isValid;
}

function validateAttachmentForm() {
    const file = $('#attachment')[0].files[0];
    if (!file) return toastr.warning("✔ Please select an Attachment."), false;
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
    const maxSize = 5 * 1024 * 1024;
    if (!allowedTypes.includes(file.type)) return toastr.warning("✔ Only JPG, PNG or PDFs are allowed."), false;
    if (file.size > maxSize) return toastr.warning("✔ File size must not exceed 5MB."), false;
    return true;
}

function showImageModal(imageUrl) {
    $('#imageModal .modal-body').html(`<img src="${imageUrl}" class="img-fluid" style="max-height: 500px;">`);
    $('#imageModal').modal('show');
}
