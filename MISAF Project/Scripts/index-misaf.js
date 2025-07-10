// ================================ Config & Routes =================================
const routes = {
    getIndex: '/MISAF/Index',
    getMisaf: '/MISAF/GetSavedMisaf',
    getUser: '/MISAF/GetUserRoles',
    getApproversForRequestor: '/MISAF/GetApprovers',
    getGroupedCategoriesRequestor: '/MISAF/GetGroupedCategories',
    getRequestForEditRequestor: '/MISAF/GetRequestForEditRequestor',
    getReasonsRequestor: '/MISAF/GetReasonsByCategory',
    getMisafDetails: '/MISAF/GetSavedDetails',
    postSaveApprove: '/MISAF/SaveApproveAsync',
    postSaveEndorse: '/MISAF/SaveEndorseAsync',
    updateRequest: '/MISAF/UpdateRequest',
    deleteRequestEdit: '/MISAF/DeleteRequestEdit',
};


// ================================ Initialize DOM =================================
$(document).ready(function () {
    initToastrRequestor();
    loadInitialTables();
    initSelect2Requestor();
    initEventsRequestor();
});

function initToastrRequestor() {
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


function loadInitialTables() {
    $.get(routes.getMisaf, function (res) {
        if (res.success) {
            console.log(res);
            loadMisafTable(res.main)
        }
    });
}


function initSelect2Requestor() {
    $('#Endorsed_By, #Final_Approver, #category').select2({
        theme: 'bootstrap-5',
        width: 'auto', 
        placeholder: 'Select an option',
        allowClear: false,
        height: '100%',
        dropdownAutoWidth: false,
        allowClear: true, 
        minimumResultsForSearch: Infinity 
    });
}

function initEventsRequestor() {
    $('#Endorsed_By').on('change', onEndorserChangeRequestor);

 
}


function onEndorserChangeRequestor() {
    $.get(routes.getApprovers, function (response) {
        if (response.success) {
            const endorser = $('#Endorsed_By').val();
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

function loadApproversForRequestor(users) {
    $.get(routes.getApproversForRequestor, function (response) {
        const approverSelect = $('#Final_Approver').empty();
        const endorserSelect = $('#Endorsed_By').empty();

        if (response.success) {
            $.each(response.data, function (i, res) {

                // check if only endorser 

                if (res.Endorser_Only == "Y") {
                    endorserSelect.append(`<option value=""></option>`);
                    endorserSelect.append(`<option  value="${res.Approver_ID}">${res.Name}</option>`);
                }

                if (res.Endorser_Only != "Y") {
                    approverSelect.append(`<option value=""></option>`);
                    approverSelect.append(`<option value="${res.Approver_ID}">${res.Name}</option>`);
                }

            });

            if (users.Endorser == 0) {
                $('#warningEndorser').text("(No Selected Endorser!)").show();
            } else {
                $('#warningEndorser').hide();
            }


            if (users.Endorser != null && users.Endorser != 0) {
                endorserSelect.val(users.Endorser).prop("disabled", false);
            }

            if (users.Approver != null) {
                approverSelect.val(users.Approver).prop("disabled", false);
            }

            approverSelect.trigger("change");
            endorserSelect.trigger("change");
        } else {
            toastr.warning(response.message);
        }
    });
}


function loadMisafTable(data) {

    const table = $('#misafTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#misafTableBody').empty();
    $.each(data, function (i, item) {
        let buttons = '';

        // can allow edit if the status is empty 
        if (item.Status != ' ' || item.Status != null) {
            buttons += `<button type="button" onclick="editMisaf('${item.MAF_No}', event)" class="btn btn-info btn-sm me-1" data-bs-toggle="tooltip" data-bs-title="Edit item"><i class="mdi mdi-pencil"></i></button>`;
        }

        buttons += `<button type="button" onclick="viewMisaf(${i}, event)" class="btn btn-info btn-sm me-1" data-bs-toggle="tooltip" data-bs-title="View item"><i class="mdi mdi-eye"></i></button>`;


        let statusDate = ''
        if (item.Status_Remarks == null) {
            statusDate = ''
        } else {
            statusDate = formatDate(item.Status_DateTime)
        }

        body.append(`<tr>
                        <td>${item.MAF_No}</td>
                        <td>${item.Requestor_Name}</td>
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

    // Initialize Bootstrap tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
}
function loadRequestTable(data) {
    const table = $('#requestTableApprover');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#requestTableBodyApprover').empty();
    $.each(data, function (i, item) {
        let buttons = '';
        let obj = {
            Category: item.Category,
            RequestProblemRecommendation: item.Request,
            Index: item.Record_ID,
            MAF_No: item.MAF_No
        }; 

        if (item.Status_Remarks == null || item.Status_Remarks == " ") {
            buttons += `<button type="button" onclick="editRequestEdit('${obj.Category}', '${obj.RequestProblemRecommendation}',${item.Record_ID}, event)" class="btn btn-info btn-sm me-1 mb-1" data-bs-toggle="tooltip" data-bs-title="Edit Request"><i class="mdi mdi-pencil"></i></button>`;
            buttons += `<button type="button" onclick="deleteRequestEdit(${obj.Index},'${obj.MAF_No}', event)" class="btn btn-danger btn-sm me-1 mb-1" data-bs-toggle="tooltip" data-bs-title="Delete Request"><i class="mdi mdi-delete"></i></button>`;
        } 

        let statusDate = ''
        if (item.Status_Remarks == null) {
            statusDate = ''
        } else {
            statusDate = formatDate(item.Status_DateTime)
        }

        body.append(`<tr>
                        <td>${item.Category}</td>
                        <td>${item.Request}</td>
                        <td>${item.Status}</td>
                        <td>${statusDate}</td>
                        <td>${item.Status_Remarks || ''}</td>
                        <td>${buttons}</td>
                    </tr>`);

    });
    table.DataTable({ bInfo: false, paging: false, searching: true, ordering: true, lengthChange: false, lengthMenu: [[50, 100], [50, 100]] });
    // Initialize Bootstrap tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
}

function loadSelectedMisaf(data, option) {
    var date1 = formatDate(data.DateTime_Endorsed);
    var date2 = formatDate(data.DateTime_Approved);
    const statusDate = formatDateToDisplay(date1)
    const approvedDate = formatDateToDisplay(date2)

    if (option == "edit") {
        $('#MAF_No').val(data.MAF_No);
        $('#Requested_By').val(data.Requestor_Name);
        $('#Requested_For').val(data.Requested_For).prop("readonly", false);
        $('#DateTime_Endorsed').val('');
        $('#Endorser_Remarks').val(data.Endorser_Remarks);
        $('#DateTime_Approved').val(approvedDate);
        $('#Final_Approver_Remarks').val(data.Final_Approver_Remarks);
        $('#Status').val(data.Status);

        if (data.PreApproved == "N") {
            $('#preApproved').prop('checked', false);
        } else {
            $('#preApproved').prop('checked', true);
        }
    }
}


function loadAttachmentTable(data) {
    const table = $('#attachmentTableApprover');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBodyApprover').empty();
    $.each(data, function (i, item) {
        const file = item.MAF_No + "-" + item.Record_ID + "-" + item.Filename;
        const ext = item.Filename.split('.').pop().toLowerCase();
        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
        // Use the controller action to serve the file
        const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageModal('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.Filename}</a>`;

        body.append(`
            <tr>
                <td>${item.Filename}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="viewAttachment(${i}, event)" class="btn btn-info btn-sm" data-bs-toggle="tooltip" data-bs-title="View"><i class="mdi mdi-eye"></i></button>
                </td>
            </tr>`);
    });
    table.DataTable({ bInfo: false, paging: false, searching: true, ordering: true, lengthChange: false, });
    $('[data-bs-toggle="tooltip"]').tooltip();
}

// Modal for image preview
function showImageModal(fileUrl) {
    const modal = $('#imageModal');
    const modalImage = modal.find('#modalImage');

    // Set the image source
    modalImage.attr('src', fileUrl);

    // Show the modal
    modal.modal('show');
}

function viewAttachment(index, event) {
    event.preventDefault();
    const item = attachmentData[index];
    const file = item.MAF_No + "-" + item.Record_ID + "-" + item.Filename;
    const ext = item.Filename.split('.').pop().toLowerCase();
    const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
    const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

    if (isImage) {
        // Show image in modal
        showImageModal(fileUrl);
    } else {
        window.open(fileUrl, '_blank');
    }
}

// ================================ Category & Reasons ==============================
//function loadReasonRequestor() {
//    $.get(routes.getReasonsRequestor, function (reasons) {
//        const reasonSelect = $('#reason').empty();
//        $.each(reasons, function (i, reason) {
//            reasonSelect.append(`<option value="${reason.Reason}">${reason.Reason}</option>`);
//        });
//        reasonSelect.trigger("change.select2");
//    });
//}

function loadGroupedCategoriesRequestor(categoryName) {
    $.get(routes.getGroupedCategoriesRequestor, function (groups) {
        const categorySelect = $('#category').empty();

        $.each(groups, function (i, group) {
            const optGroup = $('<optgroup>').attr('label', group.GroupName);
            $.each(group.Categories, function (j, category) {
                const option = $('<option>').val(category).text(category);
                // Select the option if its value matches categoryName
                if (categoryName != null && category === categoryName) {
                    option.prop('selected', true);
                }
                optGroup.append(option);
            });
            categorySelect.append(optGroup);
        });

        // Trigger select2 change event to update UI
        categorySelect.trigger('change.select2');
    });
}


// ================================ Events =================================



// ================================ Edit Management =================================
function editMisaf(id, event) {
    if (event) event.preventDefault();
    
    $.get(routes.getMisafDetails, { mafNo: id }, function (res) {
        if (res.success) {
            loadApproversForRequestor(res.users);
            $('#editViewModalLabel').text('Edit Management')
            openModalMisaf('#editViewModal', clearApproverModal)
            console.log('has a request');
            loadRequestTable(res.detail);
            loadSelectedMisaf(res.main, "edit");
            loadAttachmentTable(res.attachment)
        } else {
            toastr.info(res.message);
        }
    });
}

function editRequestEdit(Category, RequestProblemRecommendation, recordID, event) {
    if (event) event.preventDefault();
    //loadReasonRequestor();
    
    $.get(routes.getRequestForEditRequestor, { recordID: recordID }, function (res) {
        if (!res.success) {
            toastr.info(res.message);
            return;
        } else {
            loadGroupedCategoriesRequestor(res.request.Category);
            //$('#category').val(res.request.Category).trigger("change.select2");

            //setTimeout(() => $('#reason').val(data.ReasonPurpose).trigger("change"), 300);

            $('#reqProbRecom').val(res.request.Request);
            $('#remarks').val(res.request.Remarks);
            $('#editIndex').val(recordID);
            $('#requestModalLabelEdit').text("Edit Request");
            $('#requestModalEdit').modal('show');
            $('#saveButton').off('click').on('click', () => saveRequestEdit({
                Category: Category,
                RequestProblemRecommendation: RequestProblemRecommendation
            }));
        }

        
    }).fail(function () {
        toastr.error("An error occurred while trying to load the request.");
    });
}


function saveRequestEdit(originalData) {
    // Validate required fields
    const category = $('#category option:selected').text();
    const requestProblemRecommendation = $('#reqProbRecom').val().trim();
    const index = $('#editIndex').val().trim();

    if (!category) {
        toastr.error('Please select a category.', 'Validation Error');
        return;
    }
    if (!requestProblemRecommendation) {
        toastr.error('Please enter a request or recommendation.', 'Validation Error');
        return;
    }
    //if (!index) {
    //    toastr.error('Index is required.', 'Validation Error');
    //    return;
    //}

    // Check for changes
    if (originalData &&
        category === originalData.Category &&
        requestProblemRecommendation === originalData.RequestProblemRecommendation) {
        toastr.info('No changes detected.', 'Info');
        return;
    }

    // Show confirmation dialog
    if (!confirm('Are you sure you want to save the changes?')) {
        return;
    }

    const data = {
        Category: category,
        RequestProblemRecommendation: requestProblemRecommendation,
        Index: index
    };

    const token = $('input[name="__RequestVerificationToken"]').val();
    $.post(routes.updateRequest, { data, __RequestVerificationToken: token }, function (res) {
        if (!res.success) {
            toastr.info(res.message);
            return;
        }

        setTimeout(() => {
            loadRequestTable(res.data);
            $('#requestModalEdit').modal('hide');
            $('#editIndex').val('');
            toastr.success(res.message, 'Success', { timeOut: 5000 });
        }, 400);
    });
}


function deleteRequestEdit(recordID, mafNo, event) {
    if (event) event.preventDefault();

    // Show confirmation dialog
    if (!confirm('Are you sure you want to delete this request?')) {
        return;
    }
    var data = {
        Index: recordID,
        MAF_No: mafNo
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    $.post(routes.deleteRequestEdit, { data, __RequestVerificationToken: token }, function (res) {
        if (!res.success) {
            toastr.info(res.message, "Info");
            return;
        } else {
            setTimeout(() => {
                loadRequestTable(res.data);
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        }
        
    }).fail(function () {
        toastr.error("Failed to delete the request. Please try again.");
    });
}




// ================================ Modals =================================
function clearApproverModal() {

}

function clearRequestModalEdit() {
    $('#editIndex').val(-1);
    $('#category').val('').trigger("change");
    //$('#reason').val([]).trigger("change");
    $('#reqProbRecom').val('');
    $('#remarks').val('');
    $('#requestModalLabelEdit').text("Add Request");
}


function openModalMisaf(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
}
