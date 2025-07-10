// ================================ Config & Routes =================================
const routes = {
    getIndex: '/Acknowledge/Index',
    getRequestMain: '/Acknowledge/GetRequestMain',
    getRequestDetails: '/Acknowledge/GetRequestDetails',
    postSaveAcknowledge: '/Acknowledge/SaveAcknowledgeAsync',
    postSaveAcknowledgeAll: '/Acknowledge/SaveAcknowledgeAllAsync',
    postSaveApproveAll: '/Acknowledge/SaveApproveAllAsync',
    postSaveOnHoldAll: '/Acknowledge/SaveOnHoldAllAsync',
    postSaveRejectAll: '/Acknowledge/SaveRejectAllAsync',
};

// ================================ Initialize DOM =================================
$(document).ready(function () {
    initToastrAcknowledge();
    loadInitialAcknowledgeTable();
    document.getElementById("remarksModal")?.removeAttribute("aria-hidden");
    document.getElementById("remarksModal")?.focus();
    document.getElementById("approverModal")?.removeAttribute("aria-hidden");
    document.getElementById("approverModal")?.focus();
    document.getElementById("imageModal")?.removeAttribute("aria-hidden");
    document.getElementById("imageModal")?.focus();

    // Bind save buttons for remarksModal
    $('#btnSaveRemarksAcknowledge').click(function () {
        SaveAcknowledge({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: "Done"
        });
    });

    $('#btnSaveRemarksOnhold').click(function () {
        SaveAcknowledge({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: "On Hold"
        });
    });

    $('#btnSaveRemarksReject').click(function () {
        SaveAcknowledge({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: 'Reject'
        });
    });

    $('#btnAcknowledgeAll').click(function () {
        SaveAcknowledgeAll({
            MAF_No: $('#mafNoAcknowledge').val(),
            Status: 'On Going'
        });
    });

    $('#btnApproveAll').click(function () {
        SaveApproveAll({
            MAF_No: $('#mafNoAcknowledge').val(),
            Status: 'Done'
        });
    });

    $('#btnOnholdAll').click(function () {
        SaveOnHoldAll({
            MAF_No: $('#mafNoAcknowledge').val(),
            Status: 'On Hold'
        });
    });

    $('#btnRejectAll').click(function () {
        SaveRejctAll({
            MAF_No: $('#mafNoAcknowledge').val(),
            Status: 'Reject'
        });
    });
});

// Set up global AJAX pre-loader
function setupGlobalAjaxPreloader() {
    $(document).ajaxStart(function () {
        $("#loading").show();
    }).ajaxStop(function () {
        $("#loading").hide();
    });
}

function initToastrAcknowledge() {
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
        'toastClass': 'custom-toastr',
        'zIndex': 10000 // Ensure Toastr is above pre-loader
    };
}

function loadInitialAcknowledgeTable() {
    $.get(routes.getRequestMain, function (res) {
        if (res.success) {
            
            loadRequestMainTableAcknowledge(res.main, res.users);
        } else {
            toastr.error(res.message || 'Failed to load acknowledge table.', 'Error');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

function loadRequestMainTableAcknowledge(data, user) {
    const table = $('#misafTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#misafTableBody').empty();

    $.each(data, function (i, item) {

        if (item.Requestor_Name != user.MIS) {
            const statusDate = item.Status_Remarks ? formatDate(item.Status_DateTime) : '';
            const buttons = `<button type="button" class="btn btn-info btn-sm me-1 btn-acknowledge" data-maf-no="${item.MAF_No}" data-status="${item.Status}" data-bs-toggle="tooltip" data-bs-title="Endorse"><i class="mdi mdi-account-check"></i></button>`;

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
                <td>${item.MAF_No}</td>
                <td>${item.Requested_By || item.Requestor_Name}</td>
                <td>${item.Requestor_Name}</td>
                <td>${item.Status || ''}</td>
                <td>${statusDate}</td>
                <td>${item.Status_Updated_By || ''}</td>
                <td>${item.Status_Remarks || ''}</td>
                <td>${buttons}</td>
            </tr>`);
        }
    });

    table.DataTable({
        paging: true,
        searching: true,
        ordering: true,
        lengthChange: true,
        lengthMenu: [[10, 50, 100], [10, 50, 100]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();

    $('.btn-acknowledge').off('click').on('click', function (e) {
        acknowledge($(this).data('maf-no'), $(this).data('status'), e);
    });
}

function loadRequestDetailTable(data) {
    const table = $('#requestTableacknowledge');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#requestTableBodyAcknowledge').empty();
    $.each(data, function (i, item) {
        let buttons = '';

        if (item.Status === "On Going") {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1 mb-1 btn-approve" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Approve"><i class="mdi mdi-check"></i></button>`;
            buttons += `<button type="button" class="btn btn-info btn-sm me-1 mb-1 btn-onhold" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Onhold"><i class="mdi mdi-account-lock"></i></button>`;
            buttons += `<button type="button" class="btn btn-danger btn-sm me-1 mb-1 btn-reject" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Reject"><i class="mdi mdi-cancel"></i></button>`;
        }

        if (item.Status === "On Hold") {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1 mb-1 btn-approve" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Approve"><i class="mdi mdi-check"></i></button>`;
            buttons += `<button type="button" class="btn btn-danger btn-sm me-1 mb-1 btn-reject" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Reject"><i class="mdi mdi-cancel"></i></button>`;
        }

        const statusDate = (item.Status_Remarks || item.Status !== "For Approval") ? formatDate(item.Status_DateTime) : '';

        // Determine the row class based on status
        let rowClass = '';
        if (item.Status === "Approved" || item.Status === "Done") {
            rowClass = 'table-success'; 
        } else if (item.Status === "Disapproved" || item.Status === "Rejected") {
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
            <td>${item.Request}</td>
            <td>${item.Status}</td>
            <td>${statusDate}</td>
            <td>${item.Status_Remarks || ''}</td>
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

    // Bind approve/disapprove buttons
    $('.btn-approve').off('click').on('click', function () {
        approveAcknowledge($(this).data('record-id'), $(this).data('maf-no'));
    });

    $('.btn-onhold').off('click').on('click', function () {
        onhold($(this).data('record-id'), $(this).data('maf-no'));
    });

    $('.btn-reject').off('click').on('click', function () {
        disapprove($(this).data('record-id'), $(this).data('maf-no'));
    });
}

function loadSelectedRequestMainMisaf(data, details) {
    const date1 = formatDate(data.DateTime_Endorsed);
    const date2 = formatDate(data.DateTime_Approved);
    const date3 = formatDate(data.Status_DateTime);
    const statusDateEndorse = formatDateToDisplay(date1);
    const approvedDate = formatDateToDisplay(date2);
    const statusDateMain = formatDateToDisplay(date3);

    $('#MAF_No').val(data.MAF_No);
    $('#Requested_By').val(data.Requested_By || data.Requestor_Name);
    $('#Requested_For').val(data.Requested_By != null ? data.Requestor_Name : null);
    $('#Endorsed_By').val(data.Endorsed_By);
    $('#DateTime_Endorsed').val(statusDateEndorse);
    $('#Endorser_Remarks').val(data.Endorser_Remarks);
    $('#Final_Approver').val(data.Final_Approver);
    $('#DateTime_Approved').val(approvedDate);
    $('#Final_Approver_Remarks').val(data.Final_Approver_Remarks);
    $('#Status').val(data.Status);
    $('#Status_DateTime').val(statusDateMain);
    $('#mafNoAcknowledge').val(data.MAF_No);

    $('#preApproved').prop('checked', data.PreApproved === 'Y');
    if (data.Status == "On Going") {
        $('#btnAcknowledgeAll').hide();
        $('#btnApproveAll').show();
        $('#btnOnholdAll').show();
        $('#btnRejectAll').show();
    } else {
        $('#btnApproveAll').hide();
        $('#btnOnholdAll').hide();
        $('#btnRejectAll').hide();
    }

    if (data.Status == "For Acknowledgement MIS") {
        $('#btnAcknowledgeAll').show();
    }

    var cntOnhold = 0;
    var cntOnGoing = 0;
    $.each(details, function (i, item) {
        if (item.Status === 'On Hold') {
            cntOnhold++;
        }
        if (item.Status === 'On Going') {
            cntOnGoing++;
        }
    });

    if (cntOnhold > 0 && cntOnGoing == 0) {
        $('#btnOnholdAll').prop('disabled', true);
    } else {
        $('#btnOnholdAll').prop('disabled', false);
    }

}

function loadAttachmentFinalApproverTable(data) {
    const table = $('#attachmentTableApprover');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBodyApprover').empty();
    $.each(data, function (i, item) {
        const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
        const ext = item.Filename.split('.').pop().toLowerCase();
        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
        const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageModalAcknowledge('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.Filename}</a>`;

        body.append(`
            <tr>
                <td>${item.Filename}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="viewAttachmentAcknowledge(${i}, event)" class="btn btn-info btn-sm" data-bs-toggle="tooltip" data-bs-title="View"><i class="mdi mdi-eye"></i></button>
                </td>
            </tr>`);
    });

    table.DataTable({
        bInfo: false,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false
    });

    $('[data-bs-toggle="tooltip"]').tooltip();
}

function showImageModalAcknowledge(fileUrl) {
    const modal = $('#imageModal');
    const modalImage = modal.find('#modalImage');
    modalImage.attr('src', fileUrl);
    modal.modal('show');
}

function viewAttachmentAcknowledge(index, event) {
    event.preventDefault();
    const item = attachmentData[index];
    const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
    const ext = item.Filename.split('.').pop().toLowerCase();
    const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
    const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

    if (isImage) {
        showImageModalAcknowledge(fileUrl);
    } else {
        window.open(fileUrl, '_blank');
    }
}

// ================================ Acknowledgement Management =================================
function acknowledge(mafNo, status, event) {
    if (event) event.preventDefault();
    $.get(routes.getRequestDetails, { mafNo }, function (res) {
        if (res.success) {
            $('#approveModalLabel').text('Acknowledge Management');
            openModalMisaf('#approverModal', clearApproverModal);
            loadRequestDetailTable(res.detail);
            loadSelectedRequestMainMisaf(res.main, res.detail);
            loadAttachmentFinalApproverTable(res.attachment);

            $('#Status').val(status || '')
            if (status == "Done" || status == "Reject") {
                EnabledFields(true);
            }

        } else {
            toastr.info(res.message, 'Info');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

function approveAcknowledge(recordID, mafNo, event) {
    if (event) event.preventDefault();
    $('#remarksIndex').val(recordID);
    $('#remarksMAFNo').val(mafNo);
    $('#remarksStatus').val('Approved');
    $('#remarks').val('');
    $('#btnSaveRemarksAcknowledge').show();
    $('#btnSaveRemarksOnhold').hide();
    $('#btnSaveRemarksReject').hide();
    $('#remarksModal').modal('show');
}

function onhold(recordID, mafNo, event) {
    if (event) event.preventDefault();
    $('#remarksIndex').val(recordID);
    $('#remarksMAFNo').val(mafNo);
    $('#remarksStatus').val('Approved');
    $('#remarks').val('');
    $('#btnSaveRemarksAcknowledge').hide();
    $('#btnSaveRemarksOnhold').show();
    $('#btnSaveRemarksReject').hide();
    $('#remarksModal').modal('show');
}

function disapprove(recordID, mafNo, event) {
    if (event) event.preventDefault();
    $('#remarksIndex').val(recordID);
    $('#remarksMAFNo').val(mafNo);
    $('#remarksStatus').val('Disapprove');
    $('#remarks').val('');
    $('#btnSaveRemarksAcknowledge').hide();
    $('#btnSaveRemarksOnhold').hide();
    $('#btnSaveRemarksReject').show();
    $('#remarksModal').modal('show');
}

function SaveAcknowledge(originalData) {
    if (originalData.Status === "Done") {
        if (!confirm(`Are you sure you want to acknowledge this request?`)) return;
    } else if (originalData.Status === "Reject") {
        if (!confirm(`Are you sure you want to reject this request?`)) return;
    } else if (originalData.Status === "On Hold") {
        if (!confirm(`Are you sure you want to on hold this request?`)) return;
    }
    const token = $('input[name="__RequestVerificationToken"]').val();

    const request = {
        Index: originalData.Index,
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
        Remarks: $('#remarks').val()
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveAcknowledge,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 30000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveAcknowledge failed:', res);
                return;
            }

            setTimeout(() => {
                $('#Status').val(res.main.Status || '');
                if (res.main.Status == "Done" || res.main.Status == "Reject") {
                    EnabledFields(true);
                }
                loadRequestDetailTable(res.details);
                $('#Status').val(res.main.Status || '');
                var cnt = 0;
                $.each(res.details, function (i, item) {
                    if (item.Status === "On Going") {
                        cnt++;
                    }
                });

                if (cnt == 0) {
                    $('#btnOnholdAll').prop('disabled', true);
                }

                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveAcknowledge AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            $("#remarksModal").modal('hide');
            loadInitialAcknowledgeTable();
        }
    });
}

function SaveAcknowledgeAll(originalData) {
    if (originalData.Status === "On Going") {
        if (!confirm(`Are you sure you want to acknowledge request ${originalData.MAF_No}?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveAcknowledgeAll,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 10000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveAcknowledge failed:', res);

                return;
            }

            setTimeout(() => {
                $('#Status').val(res.main.Status || '');
                loadRequestDetailTable(res.details);
                var cnt = 0;
                $.each(res.details, function (i, item) {
                    if (item.Status === "For Acknowledgement MIS") {
                        cnt++;
                    }
                });

                if (cnt == 0) {
                    $('#btnAcknowledgeAll').hide();
                    $('#btnApproveAll').show();
                    $('#btnOnholdAll').show();
                    $('#btnRejectAll').show();
                }

                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveAcknowledge AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            loadInitialAcknowledgeTable();
        }
    });
}

function SaveApproveAll(originalData) {
    if (originalData.Status === "Done") {
        if (!confirm(`Are you sure you want to approve all the request?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveApproveAll,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 10000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveApprove failed:', res);
                return;
            }

            setTimeout(() => {
                $('#Status').val(res.main.Status || '');
                if (res.main.Status == "Done" || res.main.Status == "Reject") {
                    EnabledFields(true);
                }
                loadRequestDetailTable(res.details);
                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveApprove AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            loadInitialAcknowledgeTable();
        }
    });
}

function SaveOnHoldAll(originalData) {

    const tableReq = $('#requestTable');
    if ($.fn.DataTable.isDataTable(tableReq)) {
        const rowCount = tableReq.DataTable().rows().count();
        if (rowCount == 0) messages.push('✔ Please add at least one request.');
    }

    if (originalData.Status === "On Hold") {
        if (!confirm(`Are you sure you want to On Hold all the request?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveOnHoldAll,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 10000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveOnHold failed:', res);
                return;
            }

            setTimeout(() => {
                $('#Status').val(res.main.Status || '');
                if (res.main.Status == "Done" || res.main.Status == "Reject") {
                    EnabledFields(true);
                }
                loadRequestDetailTable(res.details);
                $("#btnOnholdAll").prop('disabled', true);
                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveOnHold AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            loadInitialAcknowledgeTable();
        }
    });
}

function SaveRejctAll(originalData) {
    if (originalData.Status === "Reject") {
        if (!confirm(`Are you sure you want to reject all the request?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveRejectAll,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 10000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveReject failed:', res);
                return;
            }
            setTimeout(() => {
                $('#Status').val(res.main.Status || '');
                if (res.main.Status == "Done" || res.main.Status == "Reject") {
                    EnabledFields(true);
                }
                loadRequestDetailTable(res.details);
                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveReject AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            loadInitialAcknowledgeTable();
        }
    });
}

// ================================ Modals =================================
function clearApproverModal() {
    $('#requestTableBodyAcknowledge').empty();
    $('#attachmentTableBodyApprover').empty();
    $('#MAF_No, #Requested_By, #Requested_For, #Endorsed_By, #DateTime_Endorsed, #Endorser_Remarks, #Final_Approver, #DateTime_Approved, #Final_Approver_Remarks, #Status').val('');
    $('#preApproved, #sendEmail').prop('checked', false);
}

function openModalMisaf(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
}

function EnabledFields(disabled) {
    if (disabled) {
        $('#btnApproveAll').prop('disabled', disabled)
        $('#btnOnholdAll').prop('disabled', disabled)
        $('#btnRejectAll').prop('disabled', disabled)
    } else {
        $('#btnApproveAll').prop('disabled', disabled)
        $('#btnOnholdAll').prop('disabled', disabled)
        $('#btnRejectAll').prop('disabled', disabled)
    }
}

